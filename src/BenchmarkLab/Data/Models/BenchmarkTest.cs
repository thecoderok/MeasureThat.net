using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models
{
    public partial class BenchmarkTest
    {
        public int Id { get; set; }
        public int BenchmarkVersionId { get; set; }
        public string BenchmarkText { get; set; }

        public virtual BenchmarkVersion BenchmarkVersion { get; set; }
    }
}
