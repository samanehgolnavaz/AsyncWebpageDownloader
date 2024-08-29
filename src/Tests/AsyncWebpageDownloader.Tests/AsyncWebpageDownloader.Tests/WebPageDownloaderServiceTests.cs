using AsyncWebPageDownloader.Application.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AsyncWebpageDownloader.Tests;

namespace AsyncWebPageDownloader.Tests
{
    public class WebPageDownloaderServiceTests
    {
        private readonly IConfiguration _configuration;

        public WebPageDownloaderServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"WebPageDownloader:SaveDirectory", "TestDownloadedPages"},
                {"WebPageDownloader:MaxConcurrentDownloads", "5"},
                {"WebPageDownloader:BatchSize", "10"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task DownloadWebPagesAsync_ShouldDownloadAndSavePages()
        {
            // Arrange
            var urls = new List<string>
            {
                "https://www.bcc.com",
                "https://www.apple.com",
                "https://www.google.com"
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Test content")
                }));

            var httpClient = new HttpClient(mockHttpMessageHandler);
            var service = new WebPageDownloaderService(httpClient, _configuration);

            // Act
            var results = await service.DownloadWebPagesAsync(urls);

            // Assert
            results.Should().HaveCount(3);
            results.Should().AllBeEquivalentTo("Test content");

            // Verify files are saved
            foreach (var url in urls)
            {
                var directoryName = new Uri(url).Host;
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "TestDownloadedPages", directoryName);
                var filePath = Path.Combine(directoryPath, $"{directoryName}.html");
                File.Exists(filePath).Should().BeTrue();
            }
        }

        [Fact]
        public async Task DownloadWebPagesAsync_ShouldHandleErrors()
        {
            // Arrange
            var urls = new List<string>
            {
                "https://www.bcc.com",
                "https://www.invalidurl.com"
            };

            var mockHttpMessageHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            {
                if (request.RequestUri.ToString().Contains("bcc.com"))
                {
                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("Test content")
                    });
                }
                else
                {
                    throw new HttpRequestException("Error");
                }
            });

            var httpClient = new HttpClient(mockHttpMessageHandler);
            var service = new WebPageDownloaderService(httpClient, _configuration);

            // Act
            var results = await service.DownloadWebPagesAsync(urls);

            // Assert
            results.Should().HaveCount(2);
            results[0].Should().Contain("Test content");
            results[1].Should().Contain("Error downloading https://www.invalidurl.com");
        }
    }
}
