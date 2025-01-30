namespace YouTubeDownloaderWebApp.Utility;

public static class General
{
    public const string TempDownloadsFolder = "tempDownloads";
    public static readonly string FfmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utility", "ffmpeg");
    public const int SessionAndFileExpiryTime = 15; //15 minutes
    

    public static string SanitizeText(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }


}
