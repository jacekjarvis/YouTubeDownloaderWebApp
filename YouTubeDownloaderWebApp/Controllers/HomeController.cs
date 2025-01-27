using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using YouTubeDownloaderWebApp.Models;
using YouTubeDownloaderWebApp.Utility;

namespace YouTubeDownloaderWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IYoutubeDownloader _youtubeDownloader;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _youtubeDownloader = new YoutubeExplodeDownloader();    
        }

        public IActionResult Index(YouTubeVM viewModel)
        {
            if (viewModel.URL == null) return View();

            _youtubeDownloader.VideoUrl = @"https://www.youtube.com/watch?v=ILE1KZZ0UYI";//viewModel.URL;
            try
            {
                viewModel.Title = _youtubeDownloader.GetVideoTitle();

                if (viewModel.MediaType == Utility.MediaType.Video)
                {
                    viewModel.Options =  _youtubeDownloader.GetVideoOptions().Select((option, index) => new SelectListItem
                    {
                        Text = option,
                        Value = index.ToString()
                    });
                }
                else viewModel.Options = _youtubeDownloader.GetAudioOptions().Select((option, index) => new SelectListItem
                {
                    Text = option,
                    Value = index.ToString()
                });
            }
            catch (Exception ex)
            {
                ViewBag.DataError = "Sorry. Unable to retrieve data. Try entering over a valid youtube link";
            }

            


            return View(viewModel);

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
