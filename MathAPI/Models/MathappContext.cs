using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MathAPI.Models;

public partial class MathappContext : DbContext
{
    public MathappContext()
    {
    }

    public MathappContext(DbContextOptions<MathappContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MathCalculation> MathCalculations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MathCalculation>(entity =>
        {
            entity.HasKey(e => e.CalculationId).HasName("PK__MathCalc__57C05F6661B1BA76");

            entity.Property(e => e.CalculationId).HasColumnName("CalculationID");
            entity.Property(e => e.FirebaseUuid)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasColumnName("FirebaseUUID");
            entity.Property(e => e.FirstNumber).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Result).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SecondNumber).HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
