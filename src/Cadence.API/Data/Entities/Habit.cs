namespace Cadence.API.Data.Entities;

public class Habit
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public DayOfWeek[] ScheduledDays { get; set; } = [];
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<Completion> Completions { get; set; } = [];
    public HabitStats Stats { get; set; } = null!;
}
