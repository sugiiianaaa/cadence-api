namespace Cadence.API.Domain;

/// <summary>
/// Computes the current active streak only, walking backwards from <paramref name="today"/>.
/// Grace period for today: a scheduled-but-not-yet-completed today does not break the streak.
/// For all four stats in one call (current, longest, total, last), use <see cref="StatsRecalculator"/>.
/// </summary>
public static class StreakCalculator
{
    public static int Compute(
        HashSet<DateOnly> completions,
        HashSet<DayOfWeek> scheduledDays,
        DateOnly today)
    {
        if (scheduledDays.Count == 0) return 0;

        var streak = 0;
        var current = today;
        var floor = today.AddYears(-3);

        while (current >= floor)
        {
            if (scheduledDays.Contains(current.DayOfWeek))
            {
                if (completions.Contains(current))
                {
                    streak++;
                }
                else if (current == today)
                {
                    // grace: today not yet completed doesn't break the streak
                }
                else
                {
                    break;
                }
            }

            current = current.AddDays(-1);
        }

        return streak;
    }
}
