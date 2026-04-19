using Cadence.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Completion> Completions => Set<Completion>();
    public DbSet<HabitStats>  HabitStats => Set<HabitStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(h => h.Description)
                .HasMaxLength(500);

            entity.Property(h => h.Color)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(h => h.ScheduledDays)
                .HasColumnType("jsonb");

            entity.Property(h => h.StartTime);

            entity.Property(h => h.EndTime);

            entity.Property(h => h.CreatedAt)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()");

            entity.Property(h => h.IsArchived)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<Completion>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Habit)
                .WithMany(h => h.Completions)
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.HabitId, c.Date }).IsUnique();
        });

        modelBuilder.Entity<HabitStats>(entity =>
        {
            entity.HasKey(s => s.HabitId);

            entity.HasOne(s => s.Habit)
                .WithOne(h => h.Stats)
                .HasForeignKey<HabitStats>(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
