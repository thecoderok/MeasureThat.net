using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Xml.Linq;
using System.Globalization;

namespace MeasureThat.Logic.Web.Sitemap
{
    public class SitemapGenerator
    {
        private readonly ILogger m_logger;
        private readonly IUrlHelper urlHelper;
        private readonly IHostingEnvironment environment;

        public SitemapGenerator(ILoggerFactory loggerFactory, IUrlHelper urlHelper, IHostingEnvironment environment)
        {
            this.m_logger = loggerFactory.CreateLogger<SitemapGenerator>();
            this.urlHelper = urlHelper;
            this.environment = environment;
        }

        public async Task<SitemapInfo> Generate()
        {
            var list = new List<SitemapNode>();
            AppendMainRoutes(list);
            tempWrite(list);
            return null;
        }

        public string GetSitemapDocument(IEnumerable<SitemapNode> sitemapNodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");

            foreach (SitemapNode sitemapNode in sitemapNodes)
            {
                XElement urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", Uri.EscapeUriString(sitemapNode.Url)),
                    sitemapNode.LastModified == null ? null : new XElement(
                        xmlns + "lastmod",
                        sitemapNode.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    sitemapNode.Frequency == null ? null : new XElement(
                        xmlns + "changefreq",
                        sitemapNode.Frequency.Value.ToString().ToLowerInvariant()),
                    sitemapNode.Priority == null ? null : new XElement(
                        xmlns + "priority",
                        sitemapNode.Priority.Value.ToString("F1", CultureInfo.InvariantCulture)));
                root.Add(urlElement);
            }

            XDocument document = new XDocument(root);
            return document.ToString();
        }

        private void tempWrite(List<SitemapNode> nodes)
        {
            var str = GetSitemapDocument(nodes);
            File.WriteAllText(Path.Combine(environment.WebRootPath, "sitemap.xml"), str);
        }

        private void AppendMainRoutes(List<SitemapNode> nodes)
        {
            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("Benchmark", "Home", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme)
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