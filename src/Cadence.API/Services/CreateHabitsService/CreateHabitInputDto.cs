using System.ComponentModel.DataAnnotations;
using Cadence.API.Data.ValueObjects;

namespace Cadence.API.Services.CreateHabitsService;

public record CreateHabitInputDto(
    [Required] [MaxLength(100)] string Name, 
    [MaxLength(500)] string? Description, 
    [Required][MaxLength(20)] string Color,
    TimeWindow TimeWindow,
    [Required]List<DayOfWeek> ScheduledDays);