using JetBrains.Annotations;

namespace BenchmarkLab.Models
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
