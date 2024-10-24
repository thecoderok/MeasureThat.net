using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models;

public partial class Result
{
    public long Id { get; set; }

    public long BenchmarkId { get; set; }

    public string RawUastring { get; set; }

    public string Browser { get; set; }

    public DateTime Created { get; set; }

    public string UserId { get; set; }

    public string DevicePlatform { get; set; }

    public string OperatingSystem { get; set; }

    public int Version { get; set; }

    public virtual Benchmark Benchmark { get; set; }

    public virtual ICollection<ResultRow> ResultRows { get; set; } = new List<ResultRow>();

    public virtual AspNetUser User { get; set; }
}
