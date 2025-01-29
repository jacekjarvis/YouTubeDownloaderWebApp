﻿using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter; 


namespace YouTubeDownloaderWebApp.Utility;

public class YoutubeExplodeDownloader : IYoutubeDownloader
{
    public string VideoUrl { get; set; }
    private string _videoTitle { get; set; }

    private readonly YoutubeClient _youtube;
    private List<IStreamInfo> _streams;

    public YoutubeExplodeDownloader()
    {
        _youtube = new YoutubeClient();
    }

    public string GetVideoTitle()
    {
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
            .OrderBy(stream => stream.AudioCodec)
            .ThenByDescending(stream => stream.Bitrate)
            .Select(stream => (IStreamInfo)stream).ToList();

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
        var streamInfo = _streams[option];

        var fileType = $"{streamInfo.Container}";
        var outputFilePath =  Path.Combine(outputPath, title);
        var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utility", "ffmpeg");

        if (streamInfo is AudioOnlyStreamInfo)
        {
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, $"{outputFilePath}.{fileType}");
            var mp3Converter = new MP3Converter(ffmpegPath);
            if (mp3Converter.Convert(outputFilePath, fileType))
            {
                fileType = "mp3";
            }

        }
        else
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(VideoUrl);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var streamInfos = new IStreamInfo[] { audioStreamInfo, streamInfo };
            ffmpegPath = Path.Combine(ffmpegPath, "ffmpeg.exe");
            await _youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{outputFilePath}.{fileType}").SetFFmpegPath(ffmpegPath).Build());
        }
    }

}