using System;

namespace BenchmarkLab.Models
{
    public class BenchmarkDtoForIndex
    {
        public string BenchmarkName { get; set; }

        public string Description { get; set; }

        public long Id { get; set; }

        public string OwnerId { get; set; }

        public DateTime WhenCreated { get; set; }

        public int Version { get; set; }
    }
}
