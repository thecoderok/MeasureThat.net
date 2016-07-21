using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BenchmarkLab.Models
{
    public class NewBenchmarkModel
    {
        public NewBenchmarkModel()
        {
            this.BenchmarkCode = new List<string>();
        }

        [Required]
        [Display(Name = "Benchmark Name")]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string BenchmarkName { get; set; }

        public string Description { get; set; }

        [Required]
        public uint BenchmarkVersion { get; set; }

        [Display(Name = "Html Preparation code")]
        public string HtmlPreparationCode { get; set; }

        //[Required]
        [Display(Name = "JavaScript preparation code")]
        public string ScriptPreparationCode { get; set; }

        // TODO: test must have name
        [Required]
        [Display(Name = "Benchmark code")]
        //[MinLength(2, ErrorMessage ="At least two test cases required")]
        public IEnumerable<string> BenchmarkCode { get; set; }        

        public int Id { get; set; }

        public string OwnerId { get; set; }
    }
}
