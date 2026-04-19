using Cadence.API.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cadence.API.Features.Habits;

internal static class ArchiveHabit
{
    public static async Task<Results<Ok<long>, ProblemHttpResult>> Handle(
        long habitId, AppDbContext db)
    {
        var habit = await db.Habits.FindAsync(habitId);

        if (habit is null)
            return TypedResults.Problem(detail: $"Habit {habitId} not found.", statusCode: 404);

        habit.IsArchived = true;
        await db.SaveChangesAsync();

        return TypedResults.Ok(habit.Id);
    }
}
