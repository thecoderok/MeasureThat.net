using System;
using System.Collections.Generic;

namespace MeasureThat.Net.Data.Models
{
    public partial class Result
    {
        public Result()
        {
            ResultRow = new HashSet<ResultRow>();
        }

        public long Id { get; set; }
        public long BenchmarkId { get; set; }
        public string RawUastring { get; set; }
        public string Browser { get; set; }
        public DateTime Created { get; set; }
        public string UserId { get; set; }
        public string DevicePlatform { get; set; }
        public string OperatingSystem { get; set; }

        public virtual ICollection<ResultRow> ResultRow { get; set; }
        public virtual Benchmark Benchmark { get; set; }
    }
}
