using AsyncWebpageDownloader.Application.Interfaces;
using AsyncWebpageDownloader.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncWebPageDownloader.Presentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var webPageDownloaderService = serviceProvider.GetService<IWebPageDownloaderService>();

            List<string> urls = new List<string>
            {
                "https://www.example.com",
                "https://www.microsoft.com",
                "https://www.google.com"
                // Add more URLs as needed
            };

            var tasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                tasks.Add(webPageDownloaderService.DownloadWebPageAsync(url));
            }

            var results = await Task.WhenAll(tasks);

            for (int i = 0; i < results.Length; i++)
            {
                Console.WriteLine($"Content of {urls[i]}:");
                Console.WriteLine(results[i]);
                Console.WriteLine("----------------------------------------");
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure();
        }
    }
}
