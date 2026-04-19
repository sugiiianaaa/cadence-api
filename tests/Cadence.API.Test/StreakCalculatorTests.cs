using Cadence.API.Domain;

namespace Cadence.API.Test;

public class StreakCalculatorTests
{
    private static readonly HashSet<DayOfWeek> EveryDay = new(Enum.GetValues<DayOfWeek>());
    private static readonly HashSet<DayOfWeek> MonWedFri =
        [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

    [Fact]
    public void A_habit_with_no_completions_has_no_streak()
    {
        var today = new DateOnly(2026, 4, 17);

        var streak = StreakCalculator.Compute(
            completions: [],
            scheduledDays: EveryDay,
            today: today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void A_habit_completed_today_and_the_prior_scheduled_days_counts_the_full_run()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 15),
            new(2026, 4, 16),
            new(2026, 4, 17),
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(3, streak);
    }

    [Fact]
    public void A_missed_scheduled_day_cuts_the_streak_at_the_gap()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 13),
            new(2026, 4, 14),
            // 2026-04-15 (Wed) missed
            new(2026, 4, 16),
            new(2026, 4, 17),
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void Unscheduled_days_are_skipped_without_breaking_the_streak()
    {
        var today = new DateOnly(2026, 4, 18); // Saturday, unscheduled
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 13), // Mon
            new(2026, 4, 15), // Wed
            new(2026, 4, 17), // Fri
        ];

        var streak = StreakCalculator.Compute(completions, MonWedFri, today);

        Assert.Equal(3, streak);
    }

    [Fact]
    public void The_streak_persists_across_the_year_boundary()
    {
        var today = new DateOnly(2026, 1, 2);
        HashSet<DateOnly> completions =
        [
            new(2025, 12, 30),
            new(2025, 12, 31),
            new(2026, 1, 1),
            new(2026, 1, 2),
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(4, streak);
    }

    [Fact]
    public void Today_being_scheduled_but_not_yet_completed_does_not_break_the_streak()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 15),
            new(2026, 4, 16),
            // today not yet completed
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void A_missed_day_before_today_breaks_the_streak_even_when_today_is_still_in_grace()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            new(2026, 4, 14),
            new(2026, 4, 15),
            // 2026-04-16 missed
            // today not yet completed
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(0, streak);
    }

    [Fact]
    public void Only_the_most_recent_consecutive_run_counts_toward_the_streak()
    {
        var today = new DateOnly(2026, 4, 17);
        HashSet<DateOnly> completions =
        [
            // earlier ten-day run — irrelevant
            new(2026, 3, 1), new(2026, 3, 2), new(2026, 3, 3), new(2026, 3, 4), new(2026, 3, 5),
            new(2026, 3, 6), new(2026, 3, 7), new(2026, 3, 8), new(2026, 3, 9), new(2026, 3, 10),
            // gap

            // current run
            new(2026, 4, 16),
            new(2026, 4, 17),
        ];

        var streak = StreakCalculator.Compute(completions, EveryDay, today);

        Assert.Equal(2, streak);
    }

    [Fact]
    public void A_habit_with_no_scheduled_days_has_no_streak()
    {
        var today = new DateOnly(2026, 4, 17);

        var streak = StreakCalculator.Compute(
            completions: [today],
            scheduledDays: [],
            today: today);

        Assert.Equal(0, streak);
    }
}
