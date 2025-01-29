﻿using FFMpegCore;

namespace YouTubeDownloaderWebApp.Utility;
public class MP3Converter
{
    public MP3Converter(string ffmpegPath)
    {
        GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegPath);
    }
    public bool Convert(string source, string fileType)
    {
        try
        {
            FFMpeg.ExtractAudio($"{source}.{fileType}", $"{source}.mp3");
            File.Delete($"{source}.{fileType}");
            return true;
        }
        catch
        {
            Console.WriteLine($"Error occured when converting file to mp3");
        }
        return false;
    }
}
