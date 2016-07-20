using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models
{
    public partial class Benchmark
    {
        public Benchmark()
        {
            BenchmarkVersion = new HashSet<BenchmarkVersion>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }

        public virtual ICollection<BenchmarkVersion> BenchmarkVersion { get; set; }
    }
}
