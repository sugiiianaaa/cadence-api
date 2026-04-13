using Cadence.API.Data.ValueObjects;

namespace Cadence.API.Services.GetHabitsService;

public record GetHabitOutputDto(
    long Id, 
    string Name, 
    string Color,
    TimeWindow? TimeWindow,
    List<string> ScheduledDays);