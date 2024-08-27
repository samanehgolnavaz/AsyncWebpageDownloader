using AsyncWebpageDownloader.Application.Interfaces;
using AsyncWebpageDownloader.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;



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
