namespace BenchmarkLab.Data.Dao
{
    public interface IEntityDao<TEntity, TKey>
    {
        void Add(TEntity entity);

        TEntity FindById(TKey id);

        void Delete(TEntity entity);

        void DeleteById(TKey id);
    }
}
