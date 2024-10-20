﻿using MeasureThat.Net.Data.Models;
using System;

namespace BenchmarkLab.Data.Models
{
    public partial class GenAidescription
    {
        public long Id { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public long BenchmarkId { get; set; }

        public Benchmark Benchmark { get; set; }
    }
}