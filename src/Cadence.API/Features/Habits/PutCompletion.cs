using Cadence.API.Data;
using Cadence.API.Data.Entities;
using Cadence.API.Domain;
using Cadence.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Habits;

public record PutCompletionInputDto(bool Completed);

internal static class PutCompletion
{
    public static async Task<Results<Ok<HabitStatsSnapshot>, ProblemHttpResult>> Handle(
        long habitId, DateOnly date, PutCompletionInputDto input, HttpContext ctx, AppDbContext db)
    {
        var today = UserClock.TodayFor(ctx);
        if (date > today)
            return TypedResults.Problem(
                detail: $"Cannot set completion for future date {date} (today is {today}).",
                statusCode: 400);

        var habit = await db.Habits
            .Where(h => h.Id == habitId)
            .Select(h => new { h.Id, h.ScheduledDays })
            .FirstOrDefaultAsync();

        if (habit is null)
            return TypedResults.Problem(detail: $"Habit {habitId} not found.", statusCode: 404);

        var allCompletions = await db.Completions
            .Where(c => c.HabitId == habitId)
            .ToListAsync();

        var existing = allCompletions.FirstOrDefault(c => c.Date == date);

        if (input.Completed && existing is null)
        {
            var newCompletion = new Completion { HabitId = habitId, Date = date };
            db.Completions.Add(newCompletion);
            allCompletions.Add(newCompletion);
        }
        else if (!input.Completed && existing is not null)
        {
            db.Completions.Remove(existing);
            allCompletions.Remove(existing);
        }

        var snapshot = StatsRecalculator.Recalculate(
            allCompletions.Select(c => c.Date).ToHashSet(),
            habit.ScheduledDays.ToHashSet(),
            today);

        var stats = await db.HabitStats.FirstOrDefaultAsync(s => s.HabitId == habitId);
        if (stats is null)
        {
            stats = new HabitStats { HabitId = habitId };
            db.HabitStats.Add(stats);
        }

        stats.CurrentStreak = snapshot.CurrentStreak;
        stats.LongestStreak = snapshot.LongestStreak;
        stats.TotalCompletions = snapshot.TotalCompletions;
        stats.LastCompletedDate = snapshot.LastCompletedDate;
        stats.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return TypedResults.Ok(snapshot);
    }
}
