using System.ComponentModel.DataAnnotations;
using Cadence.API.Data;
using Cadence.API.Data.Entities;
using Cadence.API.Data.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cadence.API.Features.Habits;

public record CreateHabitInputDto(
    [Required][MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Required][MaxLength(20)] string Color,
    TimeWindow TimeWindow,
    [Required] List<DayOfWeek> ScheduledDays);

internal static class CreateHabit
{
    public static async Task<Created<long>> Handle(CreateHabitInputDto input, AppDbContext db)
    {
        var habit = new Habit
        {
            Name = input.Name,
            Description = input.Description,
            Color = input.Color,
            ScheduledDays = input.ScheduledDays.ToArray(),
            IsArchived = false,
            StartTime = input.TimeWindow.Start,
            EndTime = input.TimeWindow.End,
            CreatedAt = DateTime.UtcNow,
            Completions = [],
        };

        await db.Habits.AddAsync(habit);
        await db.SaveChangesAsync();

        var stats = new HabitStats
        {
            HabitId = habit.Id,
            UpdatedAt = DateTime.UtcNow,
        };
        await db.HabitStats.AddAsync(stats);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/api/habits/{habit.Id}", habit.Id);
    }
}
