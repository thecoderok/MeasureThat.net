using BenchmarkLab.Models;

namespace BenchmarkLab.Data.Dao
{
    public interface IBenchmarksRepository : IEntityRepository<NewBenchmarkModel, int>
    {
        NewBenchmarkModel FindBenchmark(int benchmarkId, int version);
    }
}
