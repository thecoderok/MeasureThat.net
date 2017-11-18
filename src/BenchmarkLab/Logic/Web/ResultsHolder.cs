using MeasureThat.Net.Logic.Validation;
using System.Collections.Generic;

namespace BenchmarkLab.Logic.Web
{
    public class ResultsHolder<T>
    {
        public IList<T> Entities { get; set; }

        public int Page { get; set; }

        public ResultsHolder(IList<T> entities, int page)
        {
            Preconditions.ToBeNonNegative(page);
            Preconditions.ToNotBeNull(entities);
            this.Entities = entities;
            this.Page = page;
        }
    }
}
