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
using BenchmarkLab.Logic.Web.Blog;
using BenchmarkLab.Logic.Web.Sitemap;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Web;

namespace MeasureThat.Logic.Web.Sitemap
{
    public class SitemapGenerator
    {
        private readonly ILogger m_logger;
        private readonly IUrlHelper urlHelper;
        private readonly IHostingEnvironment environment;
        private readonly SqlServerBenchmarkRepository benchmarkRepository;

        private const double HighestPriority = 1.0;
        private const double HighPriority = 0.9;
        private const double DefaultPriority = 0.5;
        private const double LowPriority = 0.1;

        public SitemapGenerator(ILoggerFactory loggerFactory, IUrlHelper urlHelper, IHostingEnvironment environment, SqlServerBenchmarkRepository benchmarkRepository)
        {
            this.m_logger = loggerFactory.CreateLogger<SitemapGenerator>();
            this.urlHelper = urlHelper;
            this.environment = environment;
            this.benchmarkRepository = benchmarkRepository;
        }

        public async Task<string> Generate()
        {
            var list = new List<SitemapNode>();
            AppendMainRoutes(list);
            AppendBlogSitemap(list);
            await AppendBenchmarksToSitemap(list).ConfigureAwait(false);
            return GetSitemapDocument(list);
        }

        public string GetSitemapDocument(IEnumerable<SitemapNode> sitemapNodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");

            foreach (SitemapNode sitemapNode in sitemapNodes)
            {
                string lastMod = null;
                if (!string.IsNullOrWhiteSpace(sitemapNode.LastModifiedRaw))
                {
                    lastMod = sitemapNode.LastModifiedRaw;
                }
                else if (sitemapNode.LastModified != null)
                {
                    lastMod = sitemapNode.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz");
                }
                XElement urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", Uri.EscapeUriString(sitemapNode.Url)),
                    lastMod == null ? null : new XElement(
                        xmlns + "lastmod",
                        lastMod),
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

        private void AppendMainRoutes(List<SitemapNode> nodes)
        {
            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("", "", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Weekly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("", "Benchmarks", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Hourly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("Add", "Benchmarks", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Monthly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("Discussions", "", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Weekly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("", "blog", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = DefaultPriority,
                Frequency = SitemapFrequency.Weekly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = this.urlHelper.Action("Index", "Tools", null, this.urlHelper.ActionContext.HttpContext.Request.Scheme),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Monthly
            });
        }

        private void AppendBlogSitemap(List<SitemapNode> nodes)
        {
            string blogSitemap = BlogLocationUtil.SitemapLocation(this.environment);
            if (!File.Exists(blogSitemap))
            {
                m_logger.LogWarning("Not including Blog's sitemap as file does not exist");
                return;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Urlset), "http://www.sitemaps.org/schemas/sitemap/0.9");
                FileStream myFileStream = new FileStream(blogSitemap, FileMode.Open);
                var urls = ((Urlset)ser.Deserialize(myFileStream)).urls;
                foreach(var url in urls)
                {
                    nodes.Add(new SitemapNode()
                    {
                        Url = url.loc,
                        Frequency = SitemapFrequency.Monthly,
                        Priority = DefaultPriority,
                        LastModifiedRaw = url.lastmod,
                    });
                }
            } 
            catch(Exception e)
            {
                m_logger.LogError(e, "Error when trying to add blog's sitemap to the main one");
            }
        }

        private async Task AppendBenchmarksToSitemap(List<SitemapNode> nodes)
        {
            var benchmarks = await this.benchmarkRepository.ListAll(2000, 0).ConfigureAwait(false);
            foreach(var benchmark in benchmarks)
            {
                nodes.Add(new SitemapNode()
                {
                    LastModified = benchmark.WhenCreated,
                    Frequency = SitemapFrequency.Monthly,
                    Priority = DefaultPriority,
                    Url = this.urlHelper.Action("Show", "Benchmarks", new { id = benchmark.Id, version = benchmark.Version, name = SeoFriendlyStringConverter.Convert(benchmark.BenchmarkName) }, this.urlHelper.ActionContext.HttpContext.Request.Scheme)
                });
            }
        }
    }

    public class SitemapNode
    {
        public SitemapFrequency? Frequency { get; set; }
        public DateTime? LastModified { get; set; }
        public string LastModifiedRaw { get; set; }
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