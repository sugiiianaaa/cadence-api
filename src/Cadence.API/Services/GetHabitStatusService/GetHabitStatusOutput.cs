namespace Cadence.API.Services.GetHabitStatusService;

public record GetHabitStatusOutput(List<GetHabitStatusResult> Results);

public record GetHabitStatusResult(
    string Name,
    string Color,
    string Icon,
    string[] ScheduledDays,
    List<DateOnly> Completions,
    int Streak);
