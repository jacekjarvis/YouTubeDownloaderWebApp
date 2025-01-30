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
        var target = Path.ChangeExtension(source, "mp3");
        FFMpeg.ExtractAudio(source, target);
        File.Delete(source);
    }
}
