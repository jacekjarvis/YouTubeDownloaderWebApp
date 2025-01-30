﻿namespace YouTubeDownloaderWebApp.Utility;
public interface IYoutubeDownloader
{
    public string VideoUrl { get; set; }

    public string? GetVideoTitle();
    public IEnumerable<string> GetVideoOptions();
    public IEnumerable<string> GetAudioOptions();
    public Task DownloadMedia(int option, string outputPath, string title);

}

