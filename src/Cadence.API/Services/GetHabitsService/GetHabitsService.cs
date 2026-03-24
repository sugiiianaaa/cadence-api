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
        
        return await dbContext.Habits
            .Select(h => new GetHabitOutputDto(h.Id, h.Name, h.Color, placeholderStreak, h.ScheduledDays.ToList()))
            .ToListAsync<GetHabitOutputDto>();
    }
}