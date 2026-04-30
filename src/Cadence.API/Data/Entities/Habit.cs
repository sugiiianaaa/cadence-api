namespace Cadence.API.Data.Entities;

public class Habit
{
    public long Id { get; init; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public DayOfWeek[] ScheduledDays { get; set; } = [];
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; init; }
    public bool IsArchived { get; set; }

    public ICollection<Completion> Completions { get; init; } = [];
    public HabitStats? Stats { get; init; }
}
