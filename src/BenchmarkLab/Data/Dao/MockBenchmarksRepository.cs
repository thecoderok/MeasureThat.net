using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using BenchmarkLab.Models.BenchmarksViewModels;

namespace BenchmarkLab.Data.Dao
{
    public class MockBenchmarksRepository : IEntityRepository<NewBenchmarkModel, int>
    {
        private static int benchmarkId = 0;

        private List<NewBenchmarkModel> m_repository = new List<NewBenchmarkModel>()
        {
            new NewBenchmarkModel()
            {
                BenchmarkName = "Mock Benchmark 1",
                Description = "Mock Description",
                Id = benchmarkId++
            },

            new NewBenchmarkModel()
            {
                BenchmarkName = "йо ватсап",
                Description = "Mock Description",
                Id = benchmarkId++
            },

            new NewBenchmarkModel()
            {
                BenchmarkName = "日本語",
                Description = "Mock Description",
                Id = benchmarkId++,
                TestCases = new List<TestCase>()
                {
                    new TestCase() { BenchmarkCode = "/o/.test('Hello World!');", TestCaseName = "RegEx"},
                    new TestCase() { BenchmarkCode = "'Hello World!'.indexOf('o') > -1;", TestCaseName = "indexOf"},
                    new TestCase() { BenchmarkCode = "!!'Hello World!'.match(/o/);", TestCaseName = "match"}
                }
            },
        };


        public void Add([NotNull] NewBenchmarkModel entity)
        {
            entity.Id = benchmarkId++;
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
            return this.m_repository.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<NewBenchmarkModel> ListAll()
        {
            return this.m_repository.AsReadOnly();
        }
    }
}
