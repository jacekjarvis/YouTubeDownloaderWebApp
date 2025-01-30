using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using Microsoft.Extensions.Logging.Abstractions;


namespace YouTubeDownloaderWebApp.Utility;

public class YoutubeExplodeDownloader : IYoutubeDownloader
{
    private readonly YoutubeClient _youtube;
    private IMp3Converter _mp3Converter;

    public string? VideoUrl { get; set; }
    private string? _videoTitle { get; set; }
    private List<IStreamInfo>? _streams;
    

    public YoutubeExplodeDownloader(IMp3Converter mp3Converter)
    {
        _youtube = new YoutubeClient();
        _mp3Converter = mp3Converter;
    }

    public string? GetVideoTitle()
    {
        if (VideoUrl == null) return null;

        var video = _youtube.Videos.GetAsync(VideoUrl).Result;
        _videoTitle = video.Title;
        return video.Title;
    }


    public IEnumerable<string> GetVideoOptions()
    {
        var streamManifest = _youtube.Videos.Streams.GetManifestAsync(VideoUrl).Result;
        _streams = streamManifest.GetVideoOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .OrderByDescending(stream => stream.VideoQuality)
            .Select(stream => (IStreamInfo)stream).ToList();

        var result = _streams.Select(stream =>
        {
            var videoStream = (VideoOnlyStreamInfo)stream;
            return $"File Type: {videoStream.Container} | " +
                   $"Video Quality: {videoStream.VideoQuality} | " +
                   $"Video Resolution: {videoStream.VideoResolution} | " +
                   $"Size: {videoStream.Size} ";
        });
        return result;
    }

    public IEnumerable<string> GetAudioOptions()
    {
        var streamManifest = _youtube.Videos.Streams.GetManifestAsync(VideoUrl).Result;
        _streams = streamManifest.GetAudioOnlyStreams()
            .Where(stream => stream.Container.ToString() == "mp4")
            .OrderByDescending(stream => stream.Bitrate)
            .Select(stream => (IStreamInfo)stream).ToList();

        if (_streams == null)
        {
            _streams = streamManifest.GetAudioOnlyStreams()
                .OrderBy(stream => stream.AudioCodec)
                .ThenByDescending(stream => stream.Bitrate)
                .Select(stream => (IStreamInfo)stream).ToList();
        }

        var result = _streams.Select(stream =>
        {
            var audioStream = (AudioOnlyStreamInfo)stream;
            return $"File Type: {audioStream.Container} | " +
                   $"Audio Container: {audioStream.AudioCodec} | " +
                   $"Bitrate: {audioStream.Bitrate} | " +
                   $"Size: {audioStream.Size} ";
        });
        return result;
    }


    public async Task DownloadMedia(int option, string outputPath, string title)
    {
        if (_streams == null || _streams.Count == 0) return;

        var streamInfo = _streams[option];
        var fileExt = $".{streamInfo.Container}";
        var outputFilePath =  Path.Combine(outputPath, title, fileExt);

        if (streamInfo is AudioOnlyStreamInfo)
        {
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, outputFilePath);
            _mp3Converter.Convert(outputFilePath);
        }
        else
        {
            if (VideoUrl == null) return;

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(VideoUrl);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var streamInfos = new IStreamInfo[] { audioStreamInfo, streamInfo };
            var ffmpegPath = Path.Combine(General.FfmpegPath, "ffmpeg.exe");
            await _youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(outputFilePath).SetFFmpegPath(ffmpegPath).Build());
        }
    }

}