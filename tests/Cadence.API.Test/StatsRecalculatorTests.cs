using Cadence.API.Domain;

namespace Cadence.API.Test;

public class StatsRecalculatorTests
{
    private static readonly HashSet<DayOfWeek> EveryDay = new(Enum.GetValues<DayOfWeek>());
    private static readonly HashSet<DayOfWeek> MonWedFri =
        [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

    [Fact]
    public void A_habit_with_no_completions_has_empty_stats()
    {
        var today = new DateOnly(2026, 4, 17);

        var stats = StatsRecalculator.Recalculate(
            completions: [],
            scheduledDays: EveryDay,
            today: today);

        Assert.Equal(0, stats.CurrentStreak);
        Assert.Equal(0, stats.LongestStreak);
        Assert.Equal(0, stats.TotalCompletions);
        Assert.Null(stats.LastCompletedDate);
    }

    [Fact]
    public void Total_completions_equals_the_number_of_recorded_dates()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 1, 1),
            new(2026, 2, 14),
            new(2026, 4, 17),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(3, stats.TotalCompletions);
    }

    [Fact]
    public void Last_completed_date_is_the_most_recent_completion()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 1, 1),
            new(2026, 4, 10),
            new(2026, 2, 14),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(new DateOnly(2026, 4, 10), stats.LastCompletedDate);
    }

    [Fact]
    public void Current_streak_matches_the_live_walk_from_today()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 15),
            new(2026, 4, 16),
            new(2026, 4, 17),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(3, stats.CurrentStreak);
    }

    [Fact]
    public void Longest_streak_is_the_largest_consecutive_run_ever_recorded()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            // 10-day run in March
            new(2026, 3, 1), new(2026, 3, 2), new(2026, 3, 3), new(2026, 3, 4), new(2026, 3, 5),
            new(2026, 3, 6), new(2026, 3, 7), new(2026, 3, 8), new(2026, 3, 9), new(2026, 3, 10),
            // gap

            // 2-day run in April
            new(2026, 4, 16), new(2026, 4, 17),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(10, stats.LongestStreak);
    }

    [Fact]
    public void Longest_streak_persists_across_the_year_boundary()
    {
        var today = new DateOnly(2026, 1, 10);
        HashSet<DateOnly> completions =
        [
            new(2025, 12, 28), new(2025, 12, 29), new(2025, 12, 30), new(2025, 12, 31),
            new(2026, 1, 1), new(2026, 1, 2), new(2026, 1, 3),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(7, stats.LongestStreak);
    }

    [Fact]
    public void Longest_streak_skips_unscheduled_days_without_breaking()
    {
        // Mon/Wed/Fri habit. Two weeks of perfect completion = run of 6.
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 6),  new(2026, 4, 8),  new(2026, 4, 10), // week 1
            new(2026, 4, 13), new(2026, 4, 15), new(2026, 4, 17), // week 2
        ];

        var stats = StatsRecalculator.Recalculate(completions, MonWedFri, today);

        Assert.Equal(6, stats.LongestStreak);
    }

    [Fact]
    public void Longest_streak_is_the_max_run_across_all_gaps()
    {
        // Every-day habit. 3-day run, gap, 4-day run → longest = 4.
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 10), new(2026, 4, 11), new(2026, 4, 12),
            // 2026-04-13 missed
            new(2026, 4, 14), new(2026, 4, 15), new(2026, 4, 16), new(2026, 4, 17),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(4, stats.LongestStreak);
    }

    [Fact]
    public void Longest_streak_equals_current_streak_when_the_current_run_is_the_best_ever()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 14), new(2026, 4, 15), new(2026, 4, 16), new(2026, 4, 17),
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(4, stats.CurrentStreak);
        Assert.Equal(4, stats.LongestStreak);
    }

    [Fact]
    public void Current_streak_counts_to_the_end_of_history_when_no_gap_exists()
    {
        // User completed every scheduled day going back further than the lookup window.
        // Current streak should still equal the unbroken run, not fall back to zero.
        var today = new DateOnly(2026, 4, 17);
        var completions = new HashSet<DateOnly>();
        for (var d = today; d >= today.AddYears(-4); d = d.AddDays(-1))
            completions.Add(d);

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.True(stats.CurrentStreak >= 1000);
    }

    [Fact]
    public void Current_streak_gives_today_grace_when_scheduled_but_not_yet_completed()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 15),
            new(2026, 4, 16),
            // today not yet completed
        ];

        var stats = StatsRecalculator.Recalculate(completions, EveryDay, today);

        Assert.Equal(2, stats.CurrentStreak);
    }
}
