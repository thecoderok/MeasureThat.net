using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models
{
    public partial class Benchmark
    {
        public Benchmark()
        {
            BenchmarkTest = new HashSet<BenchmarkTest>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }
        public DateTime WhenCreated { get; set; }
        public string ScriptPreparationCode { get; set; }
        public string HtmlPreparationCode { get; set; }

        public virtual ICollection<BenchmarkTest> BenchmarkTest { get; set; }
    }
}
