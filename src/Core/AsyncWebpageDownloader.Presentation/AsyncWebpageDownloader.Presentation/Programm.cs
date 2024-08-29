using AsyncWebpageDownloader.Application.Interfaces;
using AsyncWebpageDownloader.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
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
            var configuration = serviceProvider.GetService<IConfiguration>();

            List<string> urls = configuration.GetSection("WebPageDownloader:Urls").Get<List<string>>();

            var results = await webPageDownloaderService.DownloadWebPagesAsync(urls);

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddInfrastructure();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));
        }
    }
}
