using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Logic.Web.Sitemap
{
    public class SitemapGenerator
    {
        private readonly ILogger m_logger;
        private readonly IUrlHelper urlHelper;
        private readonly IWebHostEnvironment environment;
        private readonly SqlServerBenchmarkRepository benchmarkRepository;
        private readonly IConfiguration m_configuration;
        private readonly string baseUrl;

        private const double HighestPriority = 1.0;
        private const double HighPriority = 0.9;
        private const double DefaultPriority = 0.5;
        private const double LowPriority = 0.1;
        private const string SITEMAP_CACHE = "sitemap_cached.xml";
        private readonly TimeSpan cacheDuration = TimeSpan.FromHours(6); // Set the cache duration

        public SitemapGenerator(ILoggerFactory loggerFactory, IUrlHelper urlHelper, IWebHostEnvironment environment, SqlServerBenchmarkRepository benchmarkRepository, [NotNull] IConfiguration mConfiguration)
        {
            this.m_logger = loggerFactory.CreateLogger<SitemapGenerator>();
            this.urlHelper = urlHelper;
            this.environment = environment;
            this.benchmarkRepository = benchmarkRepository;
            this.m_configuration = mConfiguration;
            this.baseUrl = mConfiguration["SiteSettings:BaseUrl"];
        }

        public async Task<string> Generate()
        {
            string sitemapPath = Path.Combine(environment.WebRootPath, SITEMAP_CACHE);
            if (File.Exists(sitemapPath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(sitemapPath);
                if (DateTime.Now - lastWriteTime < cacheDuration)
                {
                    return await File.ReadAllTextAsync(sitemapPath);
                }
            }
            var list = new List<SitemapNode>();
            AppendMainRoutes(list);
            await AppendBenchmarksToSitemap(list).ConfigureAwait(false);
            string sitemap = GetSitemapDocument(list);

            await File.WriteAllTextAsync(sitemapPath, sitemap);

            return sitemap;
        }

        public string GetSitemapDocument(IEnumerable<SitemapNode> sitemapNodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new(xmlns + "urlset");

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
                    new XElement(xmlns + "loc", Uri.EscapeDataString(sitemapNode.Url)),
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
                Url = CreateAbsoluteUrl("", "", null),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Weekly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = CreateAbsoluteUrl("", "Benchmarks", null),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Hourly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = CreateAbsoluteUrl("Add", "Benchmarks", null),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Monthly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = CreateAbsoluteUrl("Discussions", "", null),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Weekly
            });

            nodes.Add(new SitemapNode()
            {
                LastModified = DateTime.Now,
                Url = CreateAbsoluteUrl("Index", "Tools", null),
                Priority = HighestPriority,
                Frequency = SitemapFrequency.Monthly
            });
        }

        private async Task AppendBenchmarksToSitemap(List<SitemapNode> nodes)
        {
            var benchmarks = await this.benchmarkRepository.ListAllForSitemap(2000, 0).ConfigureAwait(false);
            foreach (var benchmark in benchmarks)
            {
                var whenModified = benchmark.WhenUpdated ?? benchmark.WhenCreated;
                nodes.Add(new SitemapNode()
                {
                    LastModified = benchmark.WhenCreated,
                    Frequency = SitemapFrequency.Monthly,
                    Priority = DefaultPriority,
                    Url = CreateAbsoluteUrl("Show", "Benchmarks", new { id = benchmark.Id, version = benchmark.Version, name = SeoFriendlyStringConverter.Convert(benchmark.BenchmarkName) })
                });
            }
        }

        private string CreateAbsoluteUrl(string actionName, string controllerName, object values = null)
        {
            string localhost_url = this.urlHelper.Action(actionName, controllerName, values, this.urlHelper.ActionContext.HttpContext.Request.Scheme);
            var builder = new UriBuilder(localhost_url)
            {
                Host = baseUrl
            };
            return builder.Uri.ToString();
        }

        public static Uri ReplaceBaseDomain(string originalUrl, string newBaseDomain)
        {
            Uri originalUri = new(originalUrl);
            Uri newBaseUri = new(newBaseDomain);

            // Combine the new base domain with the original path and query
            UriBuilder uriBuilder = new UriBuilder(newBaseUri)
            {
                Path = originalUri.AbsolutePath,
                Query = originalUri.Query.TrimStart('?')
            };

            return uriBuilder.Uri;
        }
    }

    public class SitemapNode
    {
        public SitemapFrequency? Frequency
        {
            get; set;
        }
        public DateTime? LastModified
        {
            get; set;
        }
        public string LastModifiedRaw
        {
            get; set;
        }
        public double? Priority
        {
            get; set;
        }
        public string Url
        {
            get; set;
        }
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