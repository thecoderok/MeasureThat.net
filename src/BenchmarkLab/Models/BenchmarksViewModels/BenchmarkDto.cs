using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeasureThat.Net.Models
{
    using BenchmarkLab.Data.Models;
    using System;

    public class BenchmarkDto
    {
        public BenchmarkDto()
        {
            this.TestCases = new List<TestCaseDto>();
        }

        [Required]
        [Display(Name = "Benchmark Name")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string BenchmarkName
        {
            get; set;
        }

        [StringLength(4000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string Description
        {
            get; set;
        }

        [Display(Name = "Html Preparation code")]
        public string HtmlPreparationCode
        {
            get; set;
        }

        [Display(Name = "JavaScript preparation code")]
        public string ScriptPreparationCode
        {
            get; set;
        }

        [Required]
        [Display(Name = "Test Cases")]
        public List<TestCaseDto> TestCases
        {
            get; set;
        }

        public long Id
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

        public DateTime? WhenUpdated
        {
            get; set;
        }

        public int Version
        {
            get; set;
        }

        public string RelatedIds
        {
            get; set;
        }

        public List<GenAidescription> LLMSummaries
        {
            get; set;
        }
    }
}
