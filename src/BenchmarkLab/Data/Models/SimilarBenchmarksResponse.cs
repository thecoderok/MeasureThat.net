using System.Collections.Generic;

namespace BenchmarkLab.Data.Models
{
    public class SimilarBenchmarksResponse
    {
        public bool HasBenchmarkWithSameName { get; set; }
        public Dictionary<string, int> BenchmarksWithSimilarName { get; set; }
    }
}
