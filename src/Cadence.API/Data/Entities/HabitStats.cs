namespace Cadence.API.Data.Entities;

public class HabitStats
{
    public long HabitId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCompletions { get; set; }
    public DateOnly? LastCompletedDate { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Habit Habit { get; set; } = null!;
}