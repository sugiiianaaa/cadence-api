namespace Cadence.API.Services.GetHabitByIdService;

public record GetHabitByIdOutputDto(
    long Id, 
    string Name,  
    string? Description, 
    string Color,
    List<string>  Schedules,
    DateTime CreatedAt,
    bool IsArchived);