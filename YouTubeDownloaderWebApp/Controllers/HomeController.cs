using System.Diagnostics;
using System.Text.Json;
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
            if (action == "Download") return await DownloadMedia(viewModel);

            return View(viewModel);
        }

        public IActionResult Download()
        {
            if (TempData["YouTubeVM"] != null)
            {
                var viewModel = JsonSerializer.Deserialize<YouTubeVM>((string)TempData["YouTubeVM"]);
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Download(YouTubeVM viewModel)
        {
            if (viewModel.FileName != null)
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, Utility.General.TempDownloadsFolder);
                path = Path.Combine(path, viewModel.FileName);
                var title = Utility.General.SanitizeText(viewModel.Title) + "." + Path.GetExtension(viewModel.FileName);

                try
                {
                    var fileBytes = System.IO.File.ReadAllBytes(path);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                    return File(fileBytes, "application/octet-stream", title);
                }
                catch (Exception ex)
                {
                    TempData["DataError"] = "Sorry, unable to process the request. Please check the YouTube link and try again.";
                    _logger.LogError(ex, "Error occurred while processing the video - Download");
                }   
            }

            return RedirectToAction("Index");
        }

        private async Task<IActionResult> DownloadMedia(YouTubeVM viewModel)
        {
            try
            {
                var selectedValue = Request.Form["Options"];
                if (!string.IsNullOrEmpty(selectedValue))
                {
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, General.TempDownloadsFolder) ; 
                    var fileName = Guid.NewGuid().ToString();
                    await _youtubeDownloader.DownloadMedia(int.Parse(selectedValue), path, fileName);

                    var ext = (viewModel.MediaType == MediaType.Video) ? ".mp4" : ".mp3";
                    fileName += ext;
                    viewModel.FileName = fileName;
                    TempData["YouTubeVM"] = JsonSerializer.Serialize(viewModel);

                    return RedirectToAction("Download");
                }
            }
            catch (Exception ex)
            {
                TempData["DataError"] = "Sorry, unable to process the request. Please check the YouTube link and try again.";
                _logger.LogError(ex, "Error occurred while processing the video - DownloadMedia");
            }
            return View(viewModel);
        }

        private void GetData(YouTubeVM viewModel)
        {
            _youtubeDownloader.VideoUrl = viewModel.URL; // @"https://www.youtube.com/watch?v=ILE1KZZ0UYI";

            try
            {
                viewModel.Title = _youtubeDownloader.GetVideoTitle();

                if (viewModel.MediaType == MediaType.Video)
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
                TempData["DataError"]  = "Sorry, unable to process the request. Please check the YouTube link and try again.";
                _logger.LogError(ex, "Error occurred while processing the video - GetData");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
