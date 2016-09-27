namespace MeasureThat.Net.Models
{
    using System.ComponentModel.DataAnnotations;

    public class TestCaseDto
    {
        //[Required]
        [Display(Name = "Test Case")]
        //[RegularExpression("[a-zA-Z0-9.`~!@#$%^&*()_\\-\\s\\+=]+")]
        //[StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string TestCaseName { get; set; }

        //[StringLength(25000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string BenchmarkCode { get; set; }
    }
}
