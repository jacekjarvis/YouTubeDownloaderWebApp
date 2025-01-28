using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YouTubeDownloaderWebApp.Utility; 

namespace YouTubeDownloaderWebApp.Models;

public class YouTubeVM
{
    [Required]
    [DisplayName("YouTube Link")]
    public string URL { get; set; }

    [Required]
    public MediaType MediaType { get; set; }

    public string? Title { get; set; }
    public IEnumerable<SelectListItem>? Options { get; set; }

    public string? FileName { get; set; }
}

