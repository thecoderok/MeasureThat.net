using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MeasureThat.Logic.Web.Sitemap
{
    public class SitemapGenerator
    {
        private readonly ILogger m_logger;

        public SitemapGenerator(ILoggerFactory loggerFactory)
        {
            this.m_logger = loggerFactory.CreateLogger<SitemapGenerator>();
        }

        public async Task<SitemapInfo> Generate(bool force)
        {
            var list = new List<SitemapNode>();
            AppendMainRoutes(list);

            return null;
        }

        private void AppendMainRoutes(List<SitemapNode> nodes)
        {
            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                //Url = 
            });
        }

        private async Task AppendBlogSitemap(List<SitemapNode> nodes)
        {
            return;
        }

        private async Task AppendBenchmarksToSitemap(List<SitemapNode> notes)
        {
            return;
        }
    }

    public class SitemapNode
    {
        public SitemapFrequency? Frequency { get; set; }
        public DateTime? LastModified { get; set; }
        public double? Priority { get; set; }
        public string Url { get; set; }
    }

    public enum SitemapFrequency
    {
        Never,
        Yearly,
        Monthly,
        Weekly,
        Daily,
        Hourly,
        Always
    }
}