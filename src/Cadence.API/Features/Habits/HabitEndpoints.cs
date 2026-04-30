namespace Cadence.API.Features.Habits;

public static class HabitEndpoints
{
    public static IEndpointRouteBuilder MapHabitEndpoints(this IEndpointRouteBuilder app)
    {
        var habits = app.MapGroup("/api/habits").WithTags("Habits");

        habits.MapGet("/", GetHabits.Handle)
            .WithName(nameof(GetHabits))
            .WithSummary("List unarchived habits")
            .WithDescription("Returns every unarchived habit with its recent completions (last 20 weeks).");

        habits.MapGet("/today", GetTodayHabits.Handle)
            .WithName(nameof(GetTodayHabits))
            .WithSummary("List today's habits")
            .WithDescription("Returns the habits scheduled for today, with completion state and current streak read from HabitStats (staleness-guarded).");

        habits.MapPost("/", CreateHabit.Handle)
            .WithName(nameof(CreateHabit))
            .WithSummary("Create a new habit")
            .WithDescription("Creates a habit and its HabitStats row. Returns the new habit's id with a Location header.");

        habits.MapDelete("/{habitId:long}", ArchiveHabit.Handle)
            .WithName(nameof(ArchiveHabit))
            .WithSummary("Archive a habit")
            .WithDescription("Soft-delete: marks the habit as archived. Archived habits disappear from list/today endpoints but keep their data. 404 if missing.");

        habits.MapPut("/{habitId:long}/completions/{date}", PutCompletion.Handle)
            .WithName(nameof(PutCompletion))
            .WithSummary("Set or unset a habit completion on a given date")
            .WithDescription("""
                Idempotent. Body: { "completed": true | false }.
                - completed=true on an already-completed date: 200, no change.
                - completed=false on a non-existing completion: 200, no change.
                - Recomputes and persists HabitStats in the same transaction.
                - 404 only if the habit doesn't exist.
                """);

        return app;
    }
}
