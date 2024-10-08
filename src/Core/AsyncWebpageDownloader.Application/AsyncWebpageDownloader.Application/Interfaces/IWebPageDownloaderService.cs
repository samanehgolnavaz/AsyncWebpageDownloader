﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebpageDownloader.Application.Interfaces
{
    public interface IWebPageDownloaderService
    {
        Task<List<string>> DownloadWebPagesAsync(List<string> urls);
    }
}
