namespace Cadence.API.Services.CreateHabitsService;

public record CreateHabitInputDto(string Name, string Description, string Color, string Icon, List<DayOfWeek> ScheduledDays);