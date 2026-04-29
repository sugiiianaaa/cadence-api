using Cadence.API.Data;
using Cadence.API.Data.ValueObjects;
using Cadence.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Habits;

public record GetTodayHabitsDto(List<TodayHabitDto> Habits);

public record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeWindow? TimeWindow);

internal static class GetTodayHabits
{
    public static async Task<Ok<GetTodayHabitsDto>> Handle(HttpContext ctx, AppDbContext db)
    {
        var today = UserClock.TodayFor(ctx);
        var todayDow = today.DayOfWeek;

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
                CurrentStreak = h.Stats != null ? h.Stats.CurrentStreak : 0,
                LastCompletedDate = h.Stats != null ? h.Stats.LastCompletedDate : null,
            })
            .ToListAsync();

        var todayHabits = habits
            .Where(h => h.ScheduledDays.Contains(todayDow))
            .Select(h => new TodayHabitDto(
                Id: h.Id,
                Name: h.Name,
                Color: h.Color,
                IsCompleted: h.LastCompletedDate == today,
                Streak: EffectiveStreak(h.CurrentStreak, h.LastCompletedDate, h.ScheduledDays.ToHashSet(), today),
                TimeWindow: new TimeWindow(h.StartTime, h.EndTime)
                )
            ).ToList();

        return TypedResults.Ok(new GetTodayHabitsDto(todayHabits));
    }

    private static int EffectiveStreak(
        int storedStreak,
        DateOnly? lastCompletedDate,
        HashSet<DayOfWeek> scheduledDays,
        DateOnly today)
    {
        if (lastCompletedDate is null)
            return 0;

        var lastRequired = MostRecentPastScheduledDay(today, scheduledDays);
        if (lastRequired is not null && lastCompletedDate < lastRequired)
            return 0;

        return storedStreak;
    }

    private static DateOnly? MostRecentPastScheduledDay(DateOnly today, HashSet<DayOfWeek> scheduledDays)
    {
        for (int i = 1; i <= 7; i++)
        {
            var d = today.AddDays(-i);
            if (scheduledDays.Contains(d.DayOfWeek))
                return d;
        }
        return null;
    }
}
