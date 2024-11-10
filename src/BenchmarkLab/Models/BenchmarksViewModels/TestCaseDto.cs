namespace MeasureThat.Net.Models
{
    using System.ComponentModel.DataAnnotations;

    public class TestCaseDto
    {
        [Display(Name = "Test Case")]
        public string TestCaseName
        {
            get; set;
        }

        public string BenchmarkCode
        {
            get; set;
        }

        public bool Deferred
        {
            get; set;
        }
    }
}
