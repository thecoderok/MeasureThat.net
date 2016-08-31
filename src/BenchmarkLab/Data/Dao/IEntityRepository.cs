using System.Collections.Generic;
using MeasureThat.Net.Models;

namespace MeasureThat.Net.Data.Dao
{
    using System.Threading.Tasks;

    public interface IEntityRepository<TModel, TKey>
    {
        Task<TKey> Add(TModel entity);

        Task<TModel> FindById(TKey id);

        Task<TKey> DeleteById(TKey id);

        Task<IEnumerable<TModel>> ListAll(int maxEntities);

        Task<IEnumerable<TModel>> ListByUser(int maxEntities, string userId);
        Task<IEnumerable<TModel>> GetLatest(int numOfItems);
    }

    // TODO: generalize that interface, do I even need it?
    public interface IResultsRepository : IEntityRepository<PublishResultsModel, long>
    {
        Task<ShowResultModel> GetResultWithBenchmark(long id);

        Task<IEnumerable<PublishResultsModel>> ListBenchmarkResults(int maxEntities, int benchmarkId);
    }
}
