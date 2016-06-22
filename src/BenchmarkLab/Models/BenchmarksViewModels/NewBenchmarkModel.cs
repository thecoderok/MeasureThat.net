using System.ComponentModel.DataAnnotations;

namespace BenchmarkLab.Models
{
    public class NewBenchmarkModel
    {
        [Required]
        [Display(Name = "Benchmark Name")]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string BenchmarkName { get; set; }

        public string Description { get; set; }

        [Required]
        public uint BenchmarkVersion { get; set; }
    }
}
