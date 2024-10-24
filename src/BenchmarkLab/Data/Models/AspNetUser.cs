using System;
using System.Collections.Generic;

namespace BenchmarkLab.Data.Models;

public partial class AspNetUser
{
    public string Id { get; set; }

    public int AccessFailedCount { get; set; }

    public string ConcurrencyStamp { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public string NormalizedEmail { get; set; }

    public string NormalizedUserName { get; set; }

    public string PasswordHash { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public string SecurityStamp { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string UserName { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<Benchmark> Benchmarks { get; set; } = new List<Benchmark>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<SaveThatBlob> SaveThatBlobs { get; set; } = new List<SaveThatBlob>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
