using JetBrains.Annotations;
using MeasureThat.Net.Data;
using MeasureThat.Net.Data.Models;
using MeasureThat.Net.Logic.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeasureThat.Net.Data.Dao
{
    public class SqlServerSaveThatBlobReporitory
    {

        private readonly ApplicationDbContext m_db;
        private readonly IMemoryCache memoryCache;

        public SqlServerSaveThatBlobReporitory([NotNull] ApplicationDbContext db, [NotNull] IMemoryCache memoryCache)
        {
            this.m_db = db;
            this.memoryCache = memoryCache;
        }

        public virtual async Task<long> Add(string ownerId)
        {
            // TODO
            var obj = new SaveThatBlob()
            {
                Name = "Test",
                OwnerId = ownerId,
                Blob = "Test content",
                Language = "js",
                WhenCreated = DateTime.UtcNow,
            };

            this.m_db.SaveThatBlob.Add(obj);
            await this.m_db.SaveChangesAsync().ConfigureAwait(false);


            return obj.Id;
        }

        public virtual async Task<IList<SaveThatBlob>> ListAll(int maxEntities, int page)
        {
            Preconditions.ToBePositive(maxEntities);
            Preconditions.ToBeNonNegative(page);

            var entities = await this.m_db.SaveThatBlob
                .OrderByDescending(t => t.WhenCreated)
                .Skip(maxEntities * page)
                .Take(maxEntities)
                .ToListAsync()
                .ConfigureAwait(false);

            return entities;
        }
    }
}
