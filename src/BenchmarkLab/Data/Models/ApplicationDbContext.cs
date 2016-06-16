using BenchmarkLab.Data.Models;
using BenchmarkLab.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BenchmarkLab.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
        {

        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Benchmark>(entity =>
            {
                entity.Property(e => e.Description).HasColumnType("nchar(400)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(60);
            });

            modelBuilder.Entity<BenchmarkTest>(entity =>
            {
                entity.Property(e => e.BenchmarkText).IsRequired();

                entity.HasOne(d => d.BenchmarkVersion)
                    .WithMany(p => p.BenchmarkTest)
                    .HasForeignKey(d => d.BenchmarkVersionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BenchmarkTest_ToBenchmarkVersion");
            });

            modelBuilder.Entity<BenchmarkVersion>(entity =>
            {
                entity.HasIndex(e => new { e.BenchmarkId, e.BenchmarkVersion1 })
                    .HasName("IX_BenchmarkVersion_Unique");

                entity.Property(e => e.BenchmarkVersion1).HasColumnName("BenchmarkVersion");

                entity.HasOne(d => d.Benchmark)
                    .WithMany(p => p.BenchmarkVersion)
                    .HasForeignKey(d => d.BenchmarkId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BenchmarkVersion_ToBenchmark");
            });
        }
        
        public virtual DbSet<Benchmark> Benchmark { get; set; }
        public virtual DbSet<BenchmarkTest> BenchmarkTest { get; set; }
        public virtual DbSet<BenchmarkVersion> BenchmarkVersion { get; set; }
    }
}