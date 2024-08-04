using System;
using System.Collections.Generic;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlitzTypes_API.Data;

public partial class BlitzTypesContext : IdentityDbContext
{
    public BlitzTypesContext()
    {
    }

    public BlitzTypesContext(DbContextOptions<BlitzTypesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EnglishWord> EnglishWords { get; set; }
    public virtual DbSet<GermanWord> GermanWords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnglishWord>(entity =>
        {
            entity.ToTable("EnglishWords");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Words)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("words");
        });

        modelBuilder.Entity<GermanWord>(entity =>
        {
            entity.ToTable("GermanWords");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Words)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("words");
        });

        //modelBuilder.ApplyConfiguration(new ApplicationUserEntityConfiguration())

        OnModelCreatingPartial(modelBuilder);
        base.OnModelCreating(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
