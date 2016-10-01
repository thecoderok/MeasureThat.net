namespace MeasureThat.Net.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public partial class ResultRow
    {
        public long Id { get; set; }
        public long ResultId { get; set; }
        public float ExecutionsPerSecond { get; set; }
        public float RelativeMarginOfError { get; set; }
        public int NumberOfSamples { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string TestName { get; set; }

        public virtual Result Result { get; set; }
    }
}
