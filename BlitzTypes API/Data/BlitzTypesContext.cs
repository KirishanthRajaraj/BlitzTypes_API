using System;
using System.Collections.Generic;
using BlitzTypes_API.Models;
using Microsoft.EntityFrameworkCore;

namespace BlitzTypes_API.Data;

public partial class BlitzTypesContext : DbContext
{
    public BlitzTypesContext()
    {
    }

    public BlitzTypesContext(DbContextOptions<BlitzTypesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EnglishWord> EnglishWords { get; set; }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnglishWord>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Words)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("words");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
