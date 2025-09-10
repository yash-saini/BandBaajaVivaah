using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BandBaaajaVivaah.Data.Models;

public partial class BandBaajaVivaahDbContext : DbContext
{
    public BandBaajaVivaahDbContext()
    {
    }

    public BandBaajaVivaahDbContext(DbContextOptions<BandBaajaVivaahDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wedding> Weddings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expenses__1445CFF32E859591");

            entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.WeddingId).HasColumnName("WeddingID");

            entity.HasOne(d => d.Wedding).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.WeddingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_Weddings");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestId).HasName("PK__Guests__0C423C32E7CB101A");

            entity.Property(e => e.GuestId).HasColumnName("GuestID");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Rsvpstatus)
                .HasMaxLength(50)
                .HasColumnName("RSVPStatus");
            entity.Property(e => e.Side).HasMaxLength(50);
            entity.Property(e => e.WeddingId).HasColumnName("WeddingID");

            entity.HasOne(d => d.Wedding).WithMany(p => p.Guests)
                .HasForeignKey(d => d.WeddingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Guests_Weddings");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949D1DA15FC38");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.WeddingId).HasColumnName("WeddingID");

            entity.HasOne(d => d.Wedding).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.WeddingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Weddings");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC169F1A0B");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534EE43EC8E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Wedding>(entity =>
        {
            entity.HasKey(e => e.WeddingId).HasName("PK__Weddings__68028BD34099BB27");

            entity.Property(e => e.WeddingId).HasColumnName("WeddingID");
            entity.Property(e => e.OwnerUserId).HasColumnName("OwnerUserID");
            entity.Property(e => e.TotalBudget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.WeddingName).HasMaxLength(150);

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.Weddings)
                .HasForeignKey(d => d.OwnerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Weddings_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
