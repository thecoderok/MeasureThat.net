using JetBrains.Annotations;

namespace MeasureThat.Net.Models
{
    public class ShowResultModel
    {
        public BenchmarkResultDto ResultModel;

        public BenchmarkDto Benchmark;

        public ShowResultModel([NotNull] BenchmarkResultDto resultModel,
            [NotNull] BenchmarkDto benchmark)
        {
            ResultModel = resultModel;
            Benchmark = benchmark;
        }
    }
}
