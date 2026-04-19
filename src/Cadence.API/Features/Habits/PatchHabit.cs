using Cadence.API.Data;
using Cadence.API.Data.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cadence.API.Features.Habits;

public record PatchHabitDto(
    string? Name,
    string? Description,
    string? Color,
    DayOfWeek[]? ScheduledDays,
    TimeWindow? TimeWindow);

internal static class PatchHabit
{
    public static async Task<Results<Ok<long>, ProblemHttpResult>> Handle(
        long habitId, PatchHabitDto input, AppDbContext db)
    {
        var habit = await db.Habits.FindAsync(habitId);

        if (habit is null)
            return TypedResults.Problem(detail: $"Habit {habitId} not found.", statusCode: 404);

        if (input.Name != null) habit.Name = input.Name;
        if (input.Color != null) habit.Color = input.Color;
        if (input.ScheduledDays != null) habit.ScheduledDays = input.ScheduledDays;
        if (input.Description != null) habit.Description = input.Description;
        if (input.TimeWindow != null)
        {
            habit.StartTime = input.TimeWindow.Start;
            habit.EndTime = input.TimeWindow.End;
        }

        await db.SaveChangesAsync();

        return TypedResults.Ok(habit.Id);
    }
}
