using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BenchmarkLab.Models
{
    public class TestCase
    {
        [JsonPropertyName("Name")]
        public string Name
        {
            get; set;
        }

        [JsonPropertyName("Code")]
        public string Code
        {
            get; set;
        }

        [JsonPropertyName("IsDeferred")]
        public bool IsDeferred
        {
            get; set;
        }
    }

    public class MeasureThatBenchmark
    {
        [JsonPropertyName("ScriptPreparationCode")]
        public string ScriptPreparationCode
        {
            get; set;
        }

        [JsonPropertyName("TestCases")]
        public List<TestCase> TestCases
        {
            get; set;
        }
    }
}
