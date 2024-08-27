
using AsyncWebpageDownloader.Application.Interfaces;
using AsyncWebPageDownloader.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;


namespace AsyncWebpageDownloader.Infrastructure
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<IWebPageDownloaderService, WebPageDownloaderService>();
            return services;
        }
    }
}
