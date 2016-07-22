using System;
using System.Collections.Generic;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using BenchmarkLab.Data.Models;

namespace BenchmarkLab.Data.Dao
{
    public class SqlServerBenchmarkRepository : IBenchmarksRepository
    {
        private readonly ApplicationDbContext m_db;

        public SqlServerBenchmarkRepository([NotNull] ApplicationDbContext db)
        {
            this.m_db = db;
        }

        public void Add(NewBenchmarkModel entity)
        {
            
        }

        public void Delete(NewBenchmarkModel entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(int id)
        {
            throw new NotImplementedException();
        }

        public NewBenchmarkModel FindBenchmark(int benchmarkId, int version)
        {
            throw new NotImplementedException();
        }

        public NewBenchmarkModel FindById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NewBenchmarkModel> ListAll()
        {
            throw new NotImplementedException();
        }
    }
}
