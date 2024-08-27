using AsyncWebpageDownloader.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebpageDownloader.Application.Services
{
    public class WebPageDownloaderService : IWebPageDownloaderService
    {
        private readonly HttpClient _httpClient;
        public WebPageDownloaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> DownloadWebPageAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error downloading {url}: {ex.Message}";
            }
        }
    }
}