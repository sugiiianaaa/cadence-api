namespace Cadence.API.Data.Entities;

public class Completion
{
    public long Id { get; set; }
    public long HabitId { get; set; }
    public DateOnly Date { get; set; }

    public Habit Habit { get; set; } = null!;
}
