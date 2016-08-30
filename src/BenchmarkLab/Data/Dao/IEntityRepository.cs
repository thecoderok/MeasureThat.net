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

        Task<IEnumerable<TModel>> ListAll(uint maxEntities);

        Task<IEnumerable<TModel>> ListByUser(uint maxEntities, string userId);
    }

    // TODO: generalize that iterface, do I even need it?
    public interface IResultsRepository : IEntityRepository<PublishResultsModel, long>
    {
        Task<ShowResultModel> GetResultWithBenchmark(long id);
    }
}
