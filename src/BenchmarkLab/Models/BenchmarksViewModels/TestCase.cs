namespace BenchmarkLab.Models.BenchmarksViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class TestCase
    {
        [Required]
        [Display(Name = "Test Case")]
        [RegularExpression("[a-zA-Z0-9.`~!@#$%^&*()_\\s]+")]
        public string TestCaseName { get; set; }

        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string BenchmarkCode { get; set; }
    }
}
