using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BenchmarkLab.Models;

namespace BenchmarkLab.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Benchmark> Blogs { get; set; }
        public DbSet<BenchmarkTestEntry> Posts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }

    public class Benchmark
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Version { get; set; }

        public string Description { get; set; }

        public string HtmlPreparationCode { get; set; }

        public string ScriptPreparationCode { get; set; }

        public List<BenchmarkTestEntry> BenchmarkTestEntries { get; set; }
    }

    public class BenchmarkTestEntry
    {
        public int Id { get; set; }

        public int BenchmarkId { get; set; }

        public Benchmark BenchmarkRef { get; set; }
    }
}
