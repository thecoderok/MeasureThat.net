using System;
using System.Collections.Generic;
using BenchmarkLab.Data.Models;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeasureThat.Net.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Benchmark> Benchmarks { get; set; }

    public virtual DbSet<BenchmarkTest> BenchmarkTests { get; set; }

    public virtual DbSet<GenAidescription> GenAidescriptions { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<ResultRow> ResultRows { get; set; }

    public virtual DbSet<SaveThatBlob> SaveThatBlobs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Benchmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC077CE16A06");

            entity.ToTable("Benchmark");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.OwnerId).HasMaxLength(450);
            entity.Property(e => e.RelatedBenchmarks).HasMaxLength(500);
            entity.Property(e => e.Version).HasDefaultValue(1);
        });

        modelBuilder.Entity<BenchmarkTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0763E2E196");

            entity.ToTable("BenchmarkTest");

            entity.HasIndex(e => e.BenchmarkId, "nci_wi_BenchmarkTest_41E00A71028C97E148C25E9CAA179FDC");

            entity.Property(e => e.BenchmarkText).IsRequired();
            entity.Property(e => e.TestName)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasOne(d => d.Benchmark).WithMany(p => p.BenchmarkTests)
                .HasForeignKey(d => d.BenchmarkId)
                .HasConstraintName("FK_BenchmarkTest_ToBenchmark");
        });

        modelBuilder.Entity<GenAidescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GenAIDes__3214EC0727A3BCB1");

            entity.ToTable("GenAIDescription");

            entity.HasIndex(e => e.BenchmarkId, "nci_msft_1_GenAIDescription_F83040212F44F8665218E15FD5BC6775");

            entity.Property(e => e.BenchmarkId).HasColumnName("BenchmarkID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Benchmark).WithMany(p => p.GenAidescriptions)
                .HasForeignKey(d => d.BenchmarkId)
                .HasConstraintName("FK_GenAI_to_benchmark");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0765711E9A");

            entity.ToTable("Result");

            entity.HasIndex(e => e.BenchmarkId, "nci_wi_Result_30A4A6472C2CA4FBBBEA149583D9D7F4");

            entity.HasIndex(e => e.BenchmarkId, "nci_wi_Result_91C7BF1B9E32D70D291795D2ADF5AF8B");

            entity.HasIndex(e => e.BenchmarkId, "nci_wi_Result_C113F5753A9A9D31C795A08C61DCAFAA");

            entity.Property(e => e.Browser).HasMaxLength(500);
            entity.Property(e => e.DevicePlatform).HasMaxLength(500);
            entity.Property(e => e.OperatingSystem).HasMaxLength(500);
            entity.Property(e => e.RawUastring)
                .IsRequired()
                .HasMaxLength(3000)
                .HasColumnName("RawUAString");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Benchmark).WithMany(p => p.Results)
                .HasForeignKey(d => d.BenchmarkId)
                .HasConstraintName("FK_Results_ToBenchmark");
        });

        modelBuilder.Entity<ResultRow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC075E196516");

            entity.ToTable("ResultRow");

            entity.HasIndex(e => e.ResultId, "nci_wi_ResultRow_75A28A3426425AE8A43649289A9FFE54");

            entity.Property(e => e.TestName)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasOne(d => d.Result).WithMany(p => p.ResultRows)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("FK_ResultRow_ToResult");
        });

        modelBuilder.Entity<SaveThatBlob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC07CD25AF6A");

            entity.ToTable("SaveThatBlob");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Blob).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(40);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.OwnerId)
                .IsRequired()
                .HasMaxLength(450);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
