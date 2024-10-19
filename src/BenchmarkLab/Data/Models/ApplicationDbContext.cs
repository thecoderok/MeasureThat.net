using BenchmarkLab.Data.Models;
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
                entity.Property(e => e.Description).HasMaxLength(4000);

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
                    .OnDelete(DeleteBehavior.Cascade)
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
                    .OnDelete(DeleteBehavior.Cascade)
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
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ResultRow_ToResult");
            });

            modelBuilder.Entity<SaveThatBlob>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Blob).IsRequired();

                entity.Property(e => e.Language).HasMaxLength(40);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(450);

                /*entity.HasOne(d => d.Owner)
                    .WithMany(p => p.SaveThatBlob)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaveThatBlob_ToUsers");*/
            });

            modelBuilder.Entity<GenAidescription>(entity =>
            {
                entity.ToTable("GenAIDescription");

                entity.Property(e => e.BenchmarkId).HasColumnName("BenchmarkID");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Model)
                    .IsRequired()
                    .HasMaxLength(100);

                //entity.HasOne(d => d.Benchmark)
                //    .WithMany(p => p.GenAidescription)
                //    .HasForeignKey(d => d.BenchmarkId)
                //    .HasConstraintName("FK_GenAI_to_benchmark");
            });

        }

        public virtual DbSet<Benchmark> Benchmark { get; set; }
        public virtual DbSet<BenchmarkTest> BenchmarkTest { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<ResultRow> ResultRow { get; set; }
        public virtual DbSet<SaveThatBlob> SaveThatBlob { get; set; }

        public virtual DbSet<GenAidescription> GenAidescription
        {
            get; set;
        }
    }
}
