using Cadence.API.Data;
using Cadence.API.Data.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.GetHabitsService;

public interface IGetHabitsService
{
    public Task<List<GetHabitOutputDto>> ExecuteAsync();
}

public class GetHabitsService(AppDbContext dbContext) 
    : IGetHabitsService
{
    public async Task<List<GetHabitOutputDto>> ExecuteAsync()
    {
        const int placeholderStreak = 0;

        var habits = await dbContext.Habits                                                       
            .Where(h => !h.IsArchived)                            
            .Select(h => new { h.Id, h.Name, h.Color, h.ScheduledDays, h.StartTime, h.EndTime })                          
            .ToListAsync();


        return habits.Select(h => new GetHabitOutputDto(
            Id:h.Id,
            Name:h.Name,
            Color:h.Color,
            TimeWindow: h.StartTime != null && h.EndTime != null
                ? new TimeWindow(h.StartTime.Value, h.EndTime.Value)
                : null,
            ScheduledDays:h.ScheduledDays.Select(d => d.ToString()).ToList())
        ).ToList();
    }
}