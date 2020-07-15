using MeasureThat.Net.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BenchmarkLab.Logic
{
    public class RelatedBooksFinder
    {
        private static AmazonLinksData data = null;
        private static Random random = new Random();
        private readonly IConfiguration m_configuration;
        private readonly bool enabled;
        private readonly int linksToRender;
        private readonly string fileWithLinks;
        private readonly ILogger m_logger;

        public RelatedBooksFinder(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.m_configuration = configuration;
            m_logger = loggerFactory.CreateLogger<RelatedBooksFinder>();
            try
            {
                this.enabled = bool.Parse(m_configuration["RelatedBooksEnabled"]);
                this.linksToRender = int.Parse(m_configuration["BookLinksToRender"]);
                this.fileWithLinks = m_configuration["FileWithLinks"];
            }
            catch (Exception e)
            {
                this.m_logger.LogError(e, "Crash while initalizing RelatedBooksFinder");
                this.enabled = false;
                this.linksToRender = 0;
                this.fileWithLinks = "";
            }
        }

        public List<AmazonLink> FindLinks(BenchmarkDto benchmark)
        {
            if (!this.enabled)
            {
                return new List<AmazonLink>();
            }
            data = GetAmazonLinksData();
            if (data == null)
            {
                return new List<AmazonLink>();
            }
            var allKeywords = data.linksWithKeywords.Keys;
            HashSet<string> foundKeywords = new HashSet<string>();
            foreach(string keyword in allKeywords)
            {
                CheckForKeyword(benchmark.BenchmarkName, keyword, foundKeywords);
                CheckForKeyword(benchmark.Description, keyword, foundKeywords);
                CheckForKeyword(benchmark.HtmlPreparationCode, keyword, foundKeywords);
                CheckForKeyword(benchmark.ScriptPreparationCode, keyword, foundKeywords);
                foreach(var test in benchmark.TestCases)
                {
                    CheckForKeyword(test.BenchmarkCode, keyword, foundKeywords);
                    CheckForKeyword(test.TestCaseName, keyword, foundKeywords);
                }
            }

            return this.FindLinksByKeywords(foundKeywords);
        }

        private void CheckForKeyword(string haystack, string keyword, HashSet<string> foundKeywords)
        {
            if (String.IsNullOrWhiteSpace(haystack))
            {
                return;
            }
            if (haystack.IndexOf(keyword, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                foundKeywords.Add(keyword);
            }
        }

        private List<AmazonLink> FindLinksByKeywords(HashSet<string> keywords)
        {
            if (!this.enabled)
            {
                return new List<AmazonLink>();
            }
            data = GetAmazonLinksData();
            if (data == null)
            {
                return new List<AmazonLink>();
            }
            var result = new List<AmazonLink>();
            foreach(var keyword in keywords)
            {
                List<AmazonLink> outList;
                if (data.linksWithKeywords.TryGetValue(keyword.ToLower().Trim(), out outList))
                {
                    result.AddRange(outList);
                }
            }
            result.AddRange(data.planLinks);
            return result.OrderBy<AmazonLink, int>((item) => random.Next()).Take(this.linksToRender).ToList();
        }

        private AmazonLinksData GetAmazonLinksData()
        {
            if (data != null)
            {
                return data;
            } 
            else
            {
                try
                {
                    data = new AmazonLinksData(this.fileWithLinks);
                }
                catch(Exception e)
                {
                    this.m_logger.LogError(e, "Crash when tryig to read books data");
                }
            }
            return data;
        }
    }
}
