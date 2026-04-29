namespace Cadence.API.Domain;

public record HabitStatsSnapshot(
    int CurrentStreak,
    int LongestStreak,
    int TotalCompletions,
    DateOnly? LastCompletedDate);

/// <summary>
/// Computes all four habit stats (current, longest, total, last) in one call.
/// Current streak delegates to <see cref="StreakCalculator"/> — grace period applies
/// (today scheduled-but-not-yet-completed does not break the streak).
/// Longest streak counts the best historical run with no grace — an incomplete today
/// never extends longest. Completions on unscheduled days are ignored.
/// </summary>
public static class StatsRecalculator
{
    public static HabitStatsSnapshot Recalculate(
        HashSet<DateOnly> completions,
        HashSet<DayOfWeek> scheduledDays,
        DateOnly today)
    {
        var valid = completions.Where(d => scheduledDays.Contains(d.DayOfWeek)).ToHashSet();

        int total = valid.Count;
        var last = total == 0 ? (DateOnly?)null : valid.Max();
        int current = StreakCalculator.Compute(valid, scheduledDays, today);
        int longest = ComputeLongestRun(valid, scheduledDays);

        return new HabitStatsSnapshot(current, longest, total, last);
    }

    private static int ComputeLongestRun(HashSet<DateOnly> completions, HashSet<DayOfWeek> scheduledDays)
    {
        if (completions.Count == 0 || scheduledDays.Count == 0)
            return 0;

        var sorted = completions.OrderBy(d => d).ToList();
        int longest = 1;
        int run = 1;

        for (int i = 1; i < sorted.Count; i++)
        {
            run = HasMissedScheduledDayBetween(sorted[i - 1], sorted[i], scheduledDays)
                ? 1
                : run + 1;

            longest = Math.Max(longest, run);
        }

        return longest;
    }

    private static bool HasMissedScheduledDayBetween(
        DateOnly earlier, DateOnly later, HashSet<DayOfWeek> scheduledDays)
    {
        for (var d = earlier.AddDays(1); d < later; d = d.AddDays(1))
            if (scheduledDays.Contains(d.DayOfWeek))
                return true;
        return false;
    }
}
