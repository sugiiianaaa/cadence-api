using Cadence.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Cadence.API.Services.GetHabitStatusService;

public interface IGetHabitStatusService
{
    Task<GetHabitStatusOutput> ExecuteAsync();
}

public class GetHabitStatusService(AppDbContext dbContext, IMemoryCache cache) : IGetHabitStatusService
{
    private static string CacheKey => $"habit-status:{DateTime.UtcNow.Year}";

    public async Task<GetHabitStatusOutput> ExecuteAsync()
    {
        if (cache.TryGetValue(CacheKey, out GetHabitStatusOutput? cached))
            return cached!;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var habits = await dbContext.Habits
            .Where(h => !h.IsArchived)
            .Select(h => new
            {
                h.Name,
                h.Color,
                h.Icon,
                h.ScheduledDays,
                Completions = h.Completions
                    .Where(c => c.Date.Year == today.Year)
                    .Select(c => c.Date)
                    .ToList()
            })
            .ToListAsync();

        var results = habits.Select(h =>
        {
            var completionSet = h.Completions.ToHashSet();
            var scheduled = h.ScheduledDays.ToHashSet();
            var streak = ComputeStreak(completionSet, scheduled, today);
            var scheduledDayNames = h.ScheduledDays.Select(d => d.ToString()).ToArray();
            return new GetHabitStatusResult(h.Name, h.Color, h.Icon, scheduledDayNames, h.Completions, streak);
        }).ToList();

        var output = new GetHabitStatusOutput(results);
        cache.Set(CacheKey, output, TimeSpan.FromHours(24));

        return output;
    }

    private static int ComputeStreak(HashSet<DateOnly> completions, HashSet<DayOfWeek> scheduled, DateOnly today)
    {
        var streak = 0;
        var current = today;

        while (current.Year == today.Year)
        {
            if (scheduled.Contains(current.DayOfWeek))
            {
                if (completions.Contains(current))
                    streak++;
                else if (current == today)
                { /* grace period — today not yet completed, don't break */ }
                else
                    break;
            }

            current = current.AddDays(-1);
        }

        return streak;
    }
}
