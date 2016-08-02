using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BenchmarkLab.Models
{
    public class PublishResultsModel
    {
        public PublishResultsModel()
        {
            this.ResultRows = new List<ResultsRowModel>();
        }

        public long Id { get; set; }

        [Required]
        public long BenchmarkId { get; set; }

        public string UserId { get; set; }

        public string DevicePlatform { get; set; }

        public string Browser { get; set; }

        public string RawUserAgenString { get; set; }

        public string OS { get; set; }

        public List<ResultsRowModel> ResultRows { get; set; }
    }

    public class ResultsRowModel
    {
        public long Id { get; set; }

        [Required]
        public string TestName { get; set; }

        [Required]
        public int NumberOfSamples { get; set; }

        [Required]
        public double ExecutionsPerSecond { get; set; }

        [Required]
        public double RelativeMarginOfError { get; set; }
    }
}
