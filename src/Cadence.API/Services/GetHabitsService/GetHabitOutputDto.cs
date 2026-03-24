namespace Cadence.API.Services.GetHabitsService;

public record GetHabitOutputDto(long Id, string Name, string Color, int Streak, List<DayOfWeek> Schedule);