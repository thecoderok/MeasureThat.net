using System.Collections.Generic;

namespace BenchmarkLab.Data.Dao
{
    public interface IEntityRepository<TEntity, TKey>
    {
        void Add(TEntity entity);

        TEntity FindById(TKey id);

        void Delete(TEntity entity);

        void DeleteById(TKey id);

        IEnumerable<TEntity> ListAll();
    }    
}
