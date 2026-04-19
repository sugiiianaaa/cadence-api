using Cadence.API.Data;
using Cadence.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Habits;

public record HeatmapDayDto(DateOnly Date, int Completed, int Total);

internal static class GetHeatmap
{
    public static async Task<Ok<List<HeatmapDayDto>>> Handle(int weeks, HttpContext ctx, AppDbContext db)
    {
        if (weeks < 1) weeks = 16;

        var today = UserClock.TodayFor(ctx);
        var from = today.AddDays(-(weeks * 7 - 1));

        var habits = await db.Habits
            .Where(h => !h.IsArchived)
            .Select(h => new
            {
                h.ScheduledDays,
                Completions = h.Completions
                    .Where(c => c.Date >= from && c.Date <= today)
                    .Select(c => c.Date)
                    .ToList(),
            })
            .ToListAsync();

        var result = new List<HeatmapDayDto>(weeks * 7);
        var current = from;

        while (current <= today)
        {
            var dow = current.DayOfWeek;
            var total = habits.Count(h => h.ScheduledDays.Contains(dow));
            var completed = habits.Sum(h => h.Completions.Contains(current) ? 1 : 0);

            result.Add(new HeatmapDayDto(current, completed, total));
            current = current.AddDays(1);
        }

        return TypedResults.Ok(result);
    }
}
