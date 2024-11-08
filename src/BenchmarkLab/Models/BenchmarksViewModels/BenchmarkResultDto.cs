using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeasureThat.Net.Models
{
    using System;

    public class BenchmarkResultDto
    {
        public BenchmarkResultDto()
        {
            this.ResultRows = new List<ResultsRowModel>();
        }

        public long Id
        {
            get; set;
        }

        [Required]
        public long BenchmarkId
        {
            get; set;
        }

        public string UserId
        {
            get; set;
        }

        public string DevicePlatform
        {
            get; set;
        }

        public string Browser
        {
            get; set;
        }

        public string RawUserAgenString
        {
            get; set;
        }

        public string OS
        {
            get; set;
        }

        public List<ResultsRowModel> ResultRows
        {
            get; set;
        }

        public DateTime WhenCreated
        {
            get; set;
        }

        public int BenchmarkVersion
        {
            get; set;
        }
    }

    public class ResultsRowModel
    {
        [Required]
        public string TestName
        {
            get; set;
        }

        [Required]
        public int NumberOfSamples
        {
            get; set;
        }

        [Required]
        public float ExecutionsPerSecond
        {
            get; set;
        }

        [Required]
        public float RelativeMarginOfError
        {
            get; set;
        }
    }
}
