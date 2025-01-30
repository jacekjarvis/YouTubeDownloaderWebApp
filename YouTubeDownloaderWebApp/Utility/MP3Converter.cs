using FFMpegCore;

namespace YouTubeDownloaderWebApp.Utility;
public class Mp3Converter : IMp3Converter
{
    public Mp3Converter(string ffmpegPath)
    {
        GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegPath);
    }
    public void Convert(string source)
    {
        var fileName = Path.GetFileNameWithoutExtension(source);
        FFMpeg.ExtractAudio(source, $"{fileName}.mp3");
        File.Delete(source);
    }
}
