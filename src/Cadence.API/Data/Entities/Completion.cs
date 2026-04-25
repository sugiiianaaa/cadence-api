namespace Cadence.API.Data.Entities;

public class Completion
{
    public long Id { get; init; }
    public long HabitId { get; init; }
    public DateOnly Date { get; init; }

    public Habit Habit { get; init; } = null!;
}
