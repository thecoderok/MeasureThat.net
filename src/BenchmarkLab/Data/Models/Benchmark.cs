using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models;

public partial class Benchmark
{
    public long Id
    {
        get; set;
    }

    public string Name
    {
        get; set;
    }

    public string Description
    {
        get; set;
    }

    public string OwnerId
    {
        get; set;
    }

    public DateTime WhenCreated
    {
        get; set;
    }

    public string ScriptPreparationCode
    {
        get; set;
    }

    public string HtmlPreparationCode
    {
        get; set;
    }

    public int Version
    {
        get; set;
    }

    public string RelatedBenchmarks
    {
        get; set;
    }

    public DateTime? WhenUpdated
    {
        get; set;
    }

    public virtual ICollection<BenchmarkTest> BenchmarkTests { get; set; } = new List<BenchmarkTest>();

    public virtual ICollection<GenAidescription> GenAidescriptions { get; set; } = new List<GenAidescription>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
