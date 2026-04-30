using Cadence.API.Data;
using Cadence.API.Data.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Habits;

public record GetHabitByIdOutputDto(
    long Id,
    string Name,
    string Color,
    List<string> ScheduledDays,
    TimeWindow? TimeWindow,
    DateTime CreatedAt,
    bool IsArchived);

internal static class GetHabitById
{
    public static async Task<Results<Ok<GetHabitByIdOutputDto>, ProblemHttpResult>> Handle(
        long habitId, AppDbContext db)
    {
        var habit = await db.Habits
            .Where(h => h.Id == habitId)
            .FirstOrDefaultAsync();

        if (habit is null)
            return TypedResults.Problem(detail: $"Habit {habitId} not found.", statusCode: 404);

        return TypedResults.Ok(new GetHabitByIdOutputDto(
            Id: habit.Id,
            Name: habit.Name,
            Color: habit.Color,
            ScheduledDays: habit.ScheduledDays.Select(d => d.ToString()).ToList(),
            TimeWindow: new TimeWindow(habit.StartTime, habit.EndTime),
            CreatedAt: habit.CreatedAt,
            IsArchived: habit.IsArchived));
    }
}
