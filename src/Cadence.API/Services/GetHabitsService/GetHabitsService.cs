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

        var habits = await dbContext.Habits.ToListAsync();

        return habits.Select(h => new GetHabitOutputDto(
                Id: h.Id,
                Name: h.Name,
                Color: h.Color,
                Streak: placeholderStreak,
                Schedules: h.ScheduledDays.Select(d => d.ToString()).ToList()))
            .ToList();
    }
}