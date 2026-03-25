using Cadence.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.GetHabitByIdService;

public interface IGetHabitByIdService
{
    Task<GetHabitByIdOutputDto?> ExecuteAsync(long habitId);
}

public class GetHabitByIdService(AppDbContext dbContext) : IGetHabitByIdService
{
    public async Task<GetHabitByIdOutputDto?> ExecuteAsync(long habitId)
    {
        var habit = await dbContext.Habits.Where(h => h.Id == habitId).FirstOrDefaultAsync();

        return habit != null 
            ? new GetHabitByIdOutputDto(habit.Id, habit.Name)
            : null;
    }
}