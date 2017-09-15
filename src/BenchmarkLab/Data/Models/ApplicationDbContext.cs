using MeasureThat.Net.Data.Models;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MeasureThat.Net.Data
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
                entity.Property(e => e.Description).HasMaxLength(400);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(450);
            });

            modelBuilder.Entity<BenchmarkTest>(entity =>
            {
                entity.Property(e => e.BenchmarkText).IsRequired();

                entity.Property(e => e.TestName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Benchmark)
                    .WithMany(p => p.BenchmarkTest)
                    .HasForeignKey(d => d.BenchmarkId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BenchmarkTest_ToBenchmark");
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.Property(e => e.Browser).HasMaxLength(100);

                entity.Property(e => e.DevicePlatform).HasMaxLength(100);

                entity.Property(e => e.OperatingSystem).HasMaxLength(100);

                entity.Property(e => e.RawUastring)
                    .IsRequired()
                    .HasColumnName("RawUAString")
                    .HasMaxLength(300);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.Benchmark)
                    .WithMany(p => p.Result)
                    .HasForeignKey(d => d.BenchmarkId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Results_ToBenchmark");
            });

            modelBuilder.Entity<ResultRow>(entity =>
            {
                entity.Property(e => e.TestName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Result)
                    .WithMany(p => p.ResultRow)
                    .HasForeignKey(d => d.ResultId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ResultRow_ToResult");
            });
        }
        
        public virtual DbSet<Benchmark> Benchmark { get; set; }
        public virtual DbSet<BenchmarkTest> BenchmarkTest { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<ResultRow> ResultRow { get; set; }
    }
}
