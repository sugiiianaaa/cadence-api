using Cadence.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.GetTodayHabitsService;

public interface IGetTodayHabitsService
{
    public Task<GetTodayHabitsDto> ExecuteAsync();
}

public class GetTodayHabitsService(AppDbContext dbContext)
    : IGetTodayHabitsService
{
    public async Task<GetTodayHabitsDto> ExecuteAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayDow = today.DayOfWeek;

        var habits = await dbContext.Habits
            .Where(h => !h.IsArchived)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.Color,
                h.ScheduledDays,
                Completions = h.Completions
                    .Where(c => c.Date.Year == today.Year)
                    .Select(c => c.Date)
                    .ToList()
            })
            .ToListAsync();

        var todayHabits = habits
            .Where(h => h.ScheduledDays.Contains(todayDow))
            .Select(h =>
            {
                var completionSet = h.Completions.ToHashSet();
                var scheduled = h.ScheduledDays.ToHashSet();
                var streak = ComputeStreak(completionSet, scheduled, today);
                var isCompleted = completionSet.Contains(today);

                return new TodayHabitDto(h.Id, h.Name, h.Color, isCompleted, streak,
                    default, default);
            })
            .ToList();

        return new GetTodayHabitsDto(todayHabits);
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
