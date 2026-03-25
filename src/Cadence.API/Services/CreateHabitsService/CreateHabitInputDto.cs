using System.ComponentModel.DataAnnotations;

namespace Cadence.API.Services.CreateHabitsService;

public record CreateHabitInputDto(
    [Required] [MaxLength(100)] string Name, 
    [MaxLength(500)] string? Description, 
    [Required][MaxLength(20)] string Color,
    [Required][MaxLength(50)] string Icon, 
    [Required]List<DayOfWeek> ScheduledDays);