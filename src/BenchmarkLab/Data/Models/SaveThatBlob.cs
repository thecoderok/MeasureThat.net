﻿using System;
using System.Collections.Generic;

namespace MeasureThat.Net.Data.Models
{
    public partial class SaveThatBlob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public DateTime WhenCreated { get; set; }
        public string Blob { get; set; }
        public string Language { get; set; }

        //public AspNetUsers Owner { get; set; }
    }
}
