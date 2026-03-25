using Cadence.API.Data;
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
            .Select(h => new { h.Id, h.Name, h.Color, h.ScheduledDays })                          
            .ToListAsync();                                                                       
   
        return habits.Select(h => new GetHabitOutputDto(                                          
                h.Id, h.Name, h.Color, placeholderStreak,             
                h.ScheduledDays.Select(d => d.ToString()).ToList()))
            .ToList();
    }
}