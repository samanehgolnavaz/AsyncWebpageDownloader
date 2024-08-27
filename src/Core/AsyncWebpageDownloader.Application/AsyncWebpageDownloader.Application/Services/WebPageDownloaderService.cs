using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncWebpageDownloader.Application.Interfaces;
using Serilog;

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
                Log.Information("Downloading web page from {Url}", url);
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();

                // Save the content to a file
                string fileName = GetFileNameFromUrl(url);
                string filePath = Path.Combine(_saveDirectory, fileName);
                await File.WriteAllTextAsync(filePath, content);

                Log.Information("Web page from {Url} downloaded and saved to {FilePath}", url, filePath);
                return content;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error downloading web page from {Url}", url);
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
