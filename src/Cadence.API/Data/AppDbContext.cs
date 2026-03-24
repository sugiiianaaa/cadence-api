using Cadence.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Completion> Completions => Set<Completion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.ScheduledDays)
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<Completion>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.Habit)
                .WithMany(h => h.Completions)
                .HasForeignKey(c => c.HabitId);
            entity.HasIndex(c => new { c.HabitId, c.Date }).IsUnique();
        });
    }
}
