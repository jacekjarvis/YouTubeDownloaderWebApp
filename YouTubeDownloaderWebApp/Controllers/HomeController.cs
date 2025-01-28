using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YouTubeDownloaderWebApp.Models;
using YouTubeDownloaderWebApp.Utility;


namespace YouTubeDownloaderWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IYoutubeDownloader _youtubeDownloader;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, IYoutubeDownloader youtubeDownloader)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _youtubeDownloader = youtubeDownloader;    
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(YouTubeVM viewModel)
        {
            GetData(viewModel);

            var action = Request.Form["action"];
            if (action == "Download") return await Download(viewModel);

            return View(viewModel);
        }

        private async Task<IActionResult> Download(YouTubeVM viewModel)
        {
            try
            {
                var selectedValue = Request.Form["Options"];
                if (!string.IsNullOrEmpty(selectedValue))
                {
                    string path = Path.GetTempPath();
                    var fileName = Guid.NewGuid().ToString();
                    var fileBytes = await _youtubeDownloader.DownloadMedia(int.Parse(selectedValue), path, fileName);

                    var ext = ".mp4";
                    if (viewModel.MediaType == Utility.MediaType.Audio) ext = ".mp3";

                    fileName += ext;
                    path = Path.Combine(path, fileName);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                    var title = SanitizeText(viewModel.Title) + ext;

                    return File(fileBytes, "application/octet-stream", title);
                }
            }
            catch (Exception ex)
            {
                ViewBag.DataError = "Sorry, unable to process the request. Please check the YouTube link and try again.";
                _logger.LogError(ex, "Error occurred while processing the video.");
            }
            return View(viewModel);
        }

        private void GetData(YouTubeVM viewModel)
        {
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
                ViewBag.DataError = "Sorry, unable to process the request. Please check the YouTube link and try again.";
                _logger.LogError(ex, "Error occurred while processing the video.");
            }
        }

        private static string SanitizeText(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
