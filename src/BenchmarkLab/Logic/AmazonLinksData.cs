using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BenchmarkLab.Logic
{
    public class AmazonLinksData
    {
        public Dictionary<string, List<AmazonLink>> linksWithKeywords { get; set; }

        public List<AmazonLink> planLinks { get; set; }

        public AmazonLinksData(string fileName)
        {
            var rawData = JsonConvert.DeserializeObject<List<AmazonLink>>(File.ReadAllText(fileName));
            this.linksWithKeywords = new Dictionary<string, List<AmazonLink>>();
            this.planLinks = new List<AmazonLink>();
            foreach(AmazonLink entry in rawData)
            {
                if (entry.Keywords.Count == 0)
                {
                    this.planLinks.Add(entry);
                }
                else
                {
                    foreach(string keyword in entry.Keywords)
                    {
                        List<AmazonLink> outList;
                        if (linksWithKeywords.TryGetValue(keyword, out outList))
                        {
                            outList.Add(entry);
                        }
                        else
                        {
                            this.linksWithKeywords.Add(keyword, new List<AmazonLink>() { entry });
                        }
                    }
                }
            }
        }
    }
}
