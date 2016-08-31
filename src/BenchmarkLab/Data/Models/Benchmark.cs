using System;
using System.Collections.Generic;

namespace MeasureThat.Net.Data.Models
{
    public partial class Benchmark
    {
        public Benchmark()
        {
            BenchmarkTest = new HashSet<BenchmarkTest>();
            Result = new HashSet<Result>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }
        public DateTime WhenCreated { get; set; }
        public string ScriptPreparationCode { get; set; }
        public string HtmlPreparationCode { get; set; }

        public virtual ICollection<BenchmarkTest> BenchmarkTest { get; set; }
        public virtual ICollection<Result> Result { get; set; }

        //public int Version { get; set; }
    }
}
