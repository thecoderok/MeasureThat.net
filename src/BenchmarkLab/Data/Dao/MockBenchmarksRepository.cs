using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkLab.Models;
using JetBrains.Annotations;

namespace BenchmarkLab.Data.Dao
{
    using System.Threading.Tasks;

    public class MockBenchmarksRepository : IEntityRepository<NewBenchmarkModel, long>
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


        public async Task<long> Add([NotNull] NewBenchmarkModel entity)
        {
            entity.Id = benchmarkId++;
            this.m_repository.Add(entity);

            // must be async
            await Task.Delay(0);
            return entity.Id;
        }

        public async Task<long> DeleteById(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<NewBenchmarkModel> FindById(long id)
        {
            await Task.Delay(0);
            return this.m_repository.FirstOrDefault(t => t.Id == id);
        }

        public async Task<IEnumerable<NewBenchmarkModel>> ListAll(uint maxEntities)
        {
            await Task.Delay(0);
            return this.m_repository.AsReadOnly();
        }

        public Task<IEnumerable<NewBenchmarkModel>> ListByUser(uint maxEntities, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
