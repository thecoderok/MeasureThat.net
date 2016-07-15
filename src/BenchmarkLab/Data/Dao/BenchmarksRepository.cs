using System;
using BenchmarkLab.Models;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace BenchmarkLab.Data.Dao
{
    public class MockBenchmarksRepository : IEntityRepository<NewBenchmarkModel, int>
    {
        private List<NewBenchmarkModel> m_repository = new List<NewBenchmarkModel>()
        {
            new NewBenchmarkModel()
            {
                BenchmarkName = "Mock Benchmark 1",
                BenchmarkVersion = 1,
                Description = "Mock Description"
            }
        };
               

        public void Add([NotNull] NewBenchmarkModel entity)
        {
            this.m_repository.Add(entity);
        }

        public void Delete(NewBenchmarkModel entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(int id)
        {
            throw new NotImplementedException();
        }

        public NewBenchmarkModel FindById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NewBenchmarkModel> ListAll()
        {
            return this.m_repository.AsReadOnly();
        }
    }
}
