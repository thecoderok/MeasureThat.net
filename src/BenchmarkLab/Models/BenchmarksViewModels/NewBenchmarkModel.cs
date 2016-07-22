using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BenchmarkLab.Models
{
    using BenchmarksViewModels;

    public class NewBenchmarkModel
    {
        public NewBenchmarkModel()
        {
            this.TestCases = new List<TestCase>();
        }

        [Required]
        [Display(Name = "Benchmark Name")]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string BenchmarkName { get; set; }

        [StringLength(60, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string Description { get; set; }

        [Required]
        public int BenchmarkVersion { get; set; }

        [Display(Name = "Html Preparation code")]
        public string HtmlPreparationCode { get; set; }

        //[Required]
        [Display(Name = "JavaScript preparation code")]
        public string ScriptPreparationCode { get; set; }

        [Required]
        [Display(Name = "Test Cases")]
        public List<TestCase> TestCases { get; set; }        

        public int Id { get; set; }

        public string OwnerId { get; set; }
    }
}
