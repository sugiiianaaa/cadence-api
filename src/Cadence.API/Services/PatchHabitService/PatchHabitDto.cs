using Cadence.API.Data.ValueObjects;

namespace Cadence.API.Services.PatchHabitService;

public record PatchHabitDto(
    string? Name, 
    string? Description,
    string? Color,
    DayOfWeek[]? ScheduledDays,
    TimeWindow? TimeWindow);