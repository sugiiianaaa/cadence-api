using System.Text.Json.Serialization;
using Cadence.API.Data;
using Cadence.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Features.Analytics;

public record GetAnalyticsOutputDto(
    string Period,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal? OverallRate,
    decimal? OverallTrend,
    [property: JsonPropertyName("weekdayProfile")]
    List<AnalyticWeekdayProfile> WeekdayProfiles,
    [property: JsonPropertyName("habits")]
    List<AnalyticHabitProfile> HabitProfiles);

public record AnalyticWeekdayProfile(DayOfWeek DayOfWeek, decimal? Rate);

public record AnalyticHabitProfile(
    long HabitId,
    string Name,
    string Color,
    decimal? Rate,
    decimal? Trend,
    decimal? RecoveryGap,
    bool NeedsAttention);

internal static class GetAnalytics
{
    public static async Task<Results<Ok<GetAnalyticsOutputDto>, ProblemHttpResult>> Handle(
        HttpContext ctx, AppDbContext db, string? period = null)
    {
        var today = UserClock.TodayFor(ctx);
        string normalized = (period ?? "month").ToLowerInvariant();

        if (!TryGetPeriodStart(normalized, today, out var periodStart))
            return TypedResults.Problem(
                detail: $"Invalid period '{period}'. Use 'week', 'month', or 'quarter'.",
                statusCode: 400);

        var periodEnd = today;
        int periodLength = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var prevPeriodEnd = periodStart.AddDays(-1);
        var prevPeriodStart = prevPeriodEnd.AddDays(-(periodLength - 1));

        var habits = await db.Habits
            .Where(h => !h.IsArchived)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.Color,
                h.ScheduledDays,
                Completions = h.Completions
                    .Where(c => c.Date >= prevPeriodStart)
                    .Select(c => c.Date)
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync();

        var perHabit = habits.Select(h =>
        {
            var completions = h.Completions.ToHashSet();
            var currentOcc = EnumerateScheduledDates(h.ScheduledDays, periodStart, periodEnd);
            var prevOcc = EnumerateScheduledDates(h.ScheduledDays, prevPeriodStart, prevPeriodEnd);
            int currentComp = currentOcc.Count(d => completions.Contains(d));
            int prevComp = prevOcc.Count(d => completions.Contains(d));
            decimal? currentRate = Rate(currentOcc.Count, currentComp);
            decimal? prevRate = Rate(prevOcc.Count, prevComp);
            return new
            {
                h.Id,
                h.Name,
                h.Color,
                h.ScheduledDays,
                Completions = completions,
                CurrentOccCount = currentOcc.Count,
                CurrentComp = currentComp,
                PrevOccCount = prevOcc.Count,
                PrevComp = prevComp,
                CurrentRate = currentRate,
                Trend = Delta(currentRate, prevRate),
            };
        }).ToList();

        var habitProfiles = perHabit.Select(p => new AnalyticHabitProfile(
            HabitId: p.Id,
            Name: p.Name,
            Color: p.Color,
            Rate: p.CurrentRate,
            Trend: p.Trend,
            RecoveryGap: ComputeRecoveryGap(p.ScheduledDays, p.Completions, periodStart, periodEnd),
            NeedsAttention: p.CurrentRate is < 0.5m && p.Trend is < -0.05m
        )).ToList();

        decimal? overallRate = Rate(perHabit.Sum(p => p.CurrentOccCount), perHabit.Sum(p => p.CurrentComp));
        decimal? prevOverallRate = Rate(perHabit.Sum(p => p.PrevOccCount), perHabit.Sum(p => p.PrevComp));
        decimal? overallTrend = Delta(overallRate, prevOverallRate);

        DayOfWeek[] mondayFirst =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
        ];

        var periodDays = Enumerable.Range(0, periodLength)
            .Select(i => periodStart.AddDays(i))
            .ToList();

        var weekdayProfiles = mondayFirst.Select(dow =>
        {
            var datesForDow = periodDays.Where(d => d.DayOfWeek == dow).ToList();
            var habitsOnDow = perHabit.Where(p => p.ScheduledDays.Contains(dow)).ToList();
            int totalOcc = datesForDow.Count * habitsOnDow.Count;
            if (totalOcc == 0)
                return new AnalyticWeekdayProfile(dow, null);
            int totalComp = habitsOnDow.Sum(p => datesForDow.Count(d => p.Completions.Contains(d)));
            return new AnalyticWeekdayProfile(dow, (decimal)totalComp / totalOcc);
        }).ToList();

        return TypedResults.Ok(new GetAnalyticsOutputDto(
            Period: normalized,
            PeriodStart: periodStart,
            PeriodEnd: periodEnd,
            OverallRate: overallRate,
            OverallTrend: overallTrend,
            WeekdayProfiles: weekdayProfiles,
            HabitProfiles: habitProfiles));
    }

    private static bool TryGetPeriodStart(string period, DateOnly today, out DateOnly start)
    {
        switch (period)
        {
            case "week":
                int offsetFromMonday = ((int)today.DayOfWeek + 6) % 7;
                start = today.AddDays(-offsetFromMonday);
                return true;
            case "month":
                start = new DateOnly(today.Year, today.Month, 1);
                return true;
            case "quarter":
                int quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                start = new DateOnly(today.Year, quarterStartMonth, 1);
                return true;
            default:
                start = default;
                return false;
        }
    }

    private static decimal? Rate(int occurrences, int completions)
        => occurrences == 0 ? null : (decimal)completions / occurrences;

    private static decimal? Delta(decimal? current, decimal? previous)
        => (current, previous) switch
        {
            (null, _) or (_, null) => null,
            var (c, p) => c - p,
        };

    private static List<DateOnly> EnumerateScheduledDates(
        DayOfWeek[] scheduledDays, DateOnly start, DateOnly end)
    {
        var scheduled = scheduledDays.ToHashSet();
        var dates = new List<DateOnly>();
        for (var d = start; d <= end; d = d.AddDays(1))
            if (scheduled.Contains(d.DayOfWeek))
                dates.Add(d);
        return dates;
    }

    private static decimal? ComputeRecoveryGap(
        DayOfWeek[] scheduledDays,
        HashSet<DateOnly> completions,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        // Spec allows recovery to land past periodEnd — walk the timeline to the
        // latest known completion so a miss can pair with a later recovery.
        var horizon = completions.Count == 0 ? periodEnd
            : completions.Max() > periodEnd ? completions.Max() : periodEnd;
        var timeline = EnumerateScheduledDates(scheduledDays, periodStart, horizon);

        var gaps = new List<int>();
        for (int i = 0; i < timeline.Count; i++)
        {
            if (timeline[i] > periodEnd)
                break;
            if (completions.Contains(timeline[i]))
                continue;
            for (int j = i + 1; j < timeline.Count; j++)
            {
                if (completions.Contains(timeline[j]))
                {
                    gaps.Add(j - i);
                    break;
                }
            }
        }

        return gaps.Count == 0 ? null : (decimal)gaps.Average();
    }
}
