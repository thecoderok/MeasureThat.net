using JetBrains.Annotations;

namespace MeasureThat.Net.Models
{
    public class ShowResultModel
    {
        public PublishResultsModel ResultModel;

        public NewBenchmarkModel Benchmark;

        public ShowResultModel([NotNull] PublishResultsModel resultModel,
            [NotNull] NewBenchmarkModel benchmark)
        {
            ResultModel = resultModel;
            Benchmark = benchmark;
        }
    }
}
