namespace YouTubeDownloaderWebApp.Utility;


public class SessionCleanupMiddleware
{
    private readonly RequestDelegate _next;
    private const int ExpiryDurationInMinutes = General.SessionAndFileExpiryTime;  // Expiry time in minutes

    public SessionCleanupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the session has expired
        if (context.Session.IsAvailable)
        {
            // Retrieve the file paths and their timestamps from the session
            var fileData = context.Session.GetObject<List<FileData>>("FilePaths");
            if (fileData != null)
            {
                // Check for expired files
                var currentTime = DateTime.Now;
                var expiredFiles = fileData.Where(f => (currentTime - f.Timestamp).TotalMinutes >= ExpiryDurationInMinutes).ToList();

                // Delete expired files
                foreach (var expiredFile in expiredFiles)
                {
                    if (System.IO.File.Exists(expiredFile.FilePath))
                    {
                        System.IO.File.Delete(expiredFile.FilePath);
                    }
                }

                // Remove expired file data from session
                fileData.RemoveAll(f => expiredFiles.Contains(f));
                context.Session.SetObject("FilePaths", fileData);
            }
        }

        await _next(context);
    }
}
/*
public class SessionCleanupMiddleware
{
    private readonly RequestDelegate _next;

    public SessionCleanupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the session has expired.
        if (!context.Session.IsAvailable)
        {
            // Retrieve the file paths from the session.
            var filePaths = context.Session.GetObject<List<string>>("FilePaths");
            if (filePaths != null)
            {
                // Delete the files.
                foreach (var filePath in filePaths)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }

        await _next(context);
    }
}
*/