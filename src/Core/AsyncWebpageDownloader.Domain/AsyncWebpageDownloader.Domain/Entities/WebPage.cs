using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebpageDownloader.Domain.Entities
{
    public class WebPage
    {
        public int Url { get; set; }
        public string Content { get; set; }
    }
}
