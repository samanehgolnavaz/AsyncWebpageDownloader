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
        private readonly int _batchSize;

        public WebPageDownloaderService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseSaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), configuration["WebPageDownloader:SaveDirectory"]);
            _maxConcurrentDownloads = int.Parse(configuration["WebPageDownloader:MaxConcurrentDownloads"]);
            _batchSize = int.Parse(configuration["WebPageDownloader:BatchSize"]);


            if (!Directory.Exists(_baseSaveDirectory))
            {
                Directory.CreateDirectory(_baseSaveDirectory);
            }
        }

        public async Task<List<string>> DownloadWebPagesAsync(List<string> urls)
        {
            var results = new List<string>();
            var semaphore = new SemaphoreSlim(_maxConcurrentDownloads);
            var tasks = new List<Task>();

            foreach (var batch in urls.Chunk(_batchSize))
            {
                foreach (var url in batch)
                {
                    tasks.Add(DownloadWebPageAsync(url, semaphore, results));
                }

                await Task.WhenAll(tasks);
                tasks.Clear();
            }

            return results;
        }

        private async Task DownloadWebPageAsync(string url, SemaphoreSlim semaphore, List<string> results)
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

                await WriteFileInChunksAsync(filePath, response.Content);

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
        }


        private async Task WriteFileInChunksAsync(string filePath, HttpContent content)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                var stream = await content.ReadAsStreamAsync();
                var buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
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
