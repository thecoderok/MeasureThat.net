namespace BenchmarkLab.Data.Models;

public partial class ResultRow
{
    public long Id
    {
        get; set;
    }

    public long ResultId
    {
        get; set;
    }

    public float ExecutionsPerSecond
    {
        get; set;
    }

    public float RelativeMarginOfError
    {
        get; set;
    }

    public int NumberOfSamples
    {
        get; set;
    }

    public string TestName
    {
        get; set;
    }

    public virtual Result Result
    {
        get; set;
    }
}
