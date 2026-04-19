using Cadence.API.Data;
using Cadence.API.Data.ValueObjects;
using Cadence.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Habits;

public record GetHabitOutputDto(
    long Id,
    string Name,
    string Color,
    TimeWindow? TimeWindow,
    List<string> ScheduledDays,
    List<DateOnly> RecentCompletions);

internal static class GetHabits
{
    public static async Task<Ok<List<GetHabitOutputDto>>> Handle(HttpContext ctx, AppDbContext db)
    {
        var today = UserClock.TodayFor(ctx);
        var from = today.AddDays(-140);

        var habits = await db.Habits
            .Where(h => !h.IsArchived)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.Color,
                h.ScheduledDays,
                h.StartTime,
                h.EndTime,
                RecentCompletions = h.Completions
                    .Where(c => c.Date >= from && c.Date <= today)
                    .Select(c => c.Date)
                    .ToList(),
            })
            .ToListAsync();

        var response = habits
            .Select(h => new GetHabitOutputDto(
                Id: h.Id,
                Name: h.Name,
                Color: h.Color,
                TimeWindow: new TimeWindow(h.StartTime, h.EndTime),
                ScheduledDays: h.ScheduledDays.Select(d => d.ToString()).ToList(),
                RecentCompletions: h.RecentCompletions))
            .ToList();

        return TypedResults.Ok(response);
    }
}
