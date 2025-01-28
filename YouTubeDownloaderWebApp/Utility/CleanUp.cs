namespace YouTubeDownloaderWebApp.Utility;

public class CleanUp
{
    private readonly ILogger<CleanUp> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CleanUp(ILogger<CleanUp> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }
    public void ClearTempDownloads()
    {
        string path = Path.Combine(_webHostEnvironment.WebRootPath, General.TempDownloadsFolder);

        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file); // Delete each file
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete file {file}");
                }
            }
        }
        _logger.LogInformation($"Clean up complete. {General.TempDownloadsFolder} directory was cleared");
    }
}
