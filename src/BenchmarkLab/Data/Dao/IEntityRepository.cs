using System.Collections.Generic;

namespace BenchmarkLab.Data.Dao
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
}
