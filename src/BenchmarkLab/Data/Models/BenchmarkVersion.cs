using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models
{
    public partial class BenchmarkVersion
    {
        public BenchmarkVersion()
        {
            BenchmarkTest = new HashSet<BenchmarkTest>();
        }

        public int Id { get; set; }
        public byte BenchmarkVersion1 { get; set; }
        public int BenchmarkId { get; set; }
        public string ScriptPreparationCode { get; set; }
        public string HtmlPreparationCode { get; set; }

        public virtual ICollection<BenchmarkTest> BenchmarkTest { get; set; }
        public virtual Benchmark Benchmark { get; set; }
    }
}
