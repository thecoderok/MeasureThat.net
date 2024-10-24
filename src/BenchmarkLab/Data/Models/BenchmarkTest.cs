using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models;

public partial class BenchmarkTest
{
    public long Id { get; set; }

    public long BenchmarkId { get; set; }

    public string BenchmarkText { get; set; }

    public string TestName { get; set; }

    public virtual Benchmark Benchmark { get; set; }
}
