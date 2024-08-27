using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncWebpageDownloader.Application.Interfaces;

namespace AsyncWebPageDownloader.Application.Services
{
    public class WebPageDownloaderService : IWebPageDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _saveDirectory;

        public WebPageDownloaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedPages");

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        public async Task<string> DownloadWebPageAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();

                // Save the content to a file
                string fileName = GetFileNameFromUrl(url);
                string filePath = Path.Combine(_saveDirectory, fileName);
                await File.WriteAllTextAsync(filePath, content);

                return content;
            }
            catch (Exception ex)
            {
                return $"Error downloading {url}: {ex.Message}";
            }
        }

        private string GetFileNameFromUrl(string url)
        {
            // Generate a filename based on the URL
            return $"{new Uri(url).Host}.html";
        }
    }
}
