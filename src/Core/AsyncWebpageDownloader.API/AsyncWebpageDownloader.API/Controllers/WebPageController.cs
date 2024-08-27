using AsyncWebpageDownloader.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncWebPageDownloader.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebPageController : ControllerBase
    {
        private readonly IWebPageDownloaderService _webPageDownloaderService;

        public WebPageController(IWebPageDownloaderService webPageDownloaderService)
        {
            _webPageDownloaderService = webPageDownloaderService;
        }

        [HttpPost("download")]
        public async Task<IActionResult> DownloadWebPages([FromBody] List<string> urls)
        {
            //var tasks = new List<Task<string>>();

            //foreach (var url in urls)
            //{
            //    tasks.Add(_webPageDownloaderService.DownloadWebPageAsync(url));
            //}

            //var results = await Task.WhenAll(tasks);
            var results = await _webPageDownloaderService.DownloadWebPagesAsync(urls);

            return Ok(results);
        }
    }
}

