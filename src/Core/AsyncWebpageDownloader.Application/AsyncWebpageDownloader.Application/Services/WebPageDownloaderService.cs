using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncWebpageDownloader.Application.Interfaces;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AsyncWebPageDownloader.Application.Services
{
    public class WebPageDownloaderService : IWebPageDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseSaveDirectory;
        private readonly int _maxConcurrentDownloads;

        public WebPageDownloaderService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseSaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), configuration["WebPageDownloader:SaveDirectory"]);
            _maxConcurrentDownloads = int.Parse(configuration["WebPageDownloader:MaxConcurrentDownloads"]);

            if (!Directory.Exists(_baseSaveDirectory))
            {
                Directory.CreateDirectory(_baseSaveDirectory);
            }
        }

        public async Task<List<string>> DownloadWebPagesAsync(List<string> urls)
        {
            var results = new List<string>();
            var semaphore = new SemaphoreSlim(_maxConcurrentDownloads);

            var tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Log.Information("Downloading web page from {Url}", url);
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();

                    // Create a new directory for each downloaded page
                    string directoryName = GetDirectoryNameFromUrl(url);
                    string directoryPath = Path.Combine(_baseSaveDirectory, directoryName);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Save the content to a file
                    string fileName = GetFileNameFromUrl(url);
                    string filePath = Path.Combine(directoryPath, fileName);
                    await File.WriteAllTextAsync(filePath, content);

                    Log.Information("Web page from {Url} downloaded and saved to {FilePath}", url, filePath);
                    results.Add(content);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error downloading web page from {Url}", url);
                    results.Add($"Error downloading {url}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        private string GetDirectoryNameFromUrl(string url)
        {
            // Generate a directory name based on the URL
            return new Uri(url).Host;
        }

        private string GetFileNameFromUrl(string url)
        {
            // Generate a filename based on the URL
            return $"{new Uri(url).Host}.html";
        }
    }
}
