using Cadence.API.Data;
using Cadence.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.CreateHabitCompletionService;

public interface ICreateHabitCompletionService
{
    public Task<long?> ExecuteAsync(long habitId);
}

public class CreateHabitCompletionService(AppDbContext dbContext) : ICreateHabitCompletionService
{
    public async Task<long?> ExecuteAsync(long habitId)
    {
        var habitExists = await dbContext.Habits.AnyAsync(h => h.Id == habitId);

        if (!habitExists)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alreadyCompleted = await dbContext.Completions
            .AnyAsync(c => c.HabitId == habitId && c.Date == today);

        if (alreadyCompleted)
            return null;

        var completion = new Completion
        {
            HabitId = habitId,
            Date = today
        };

        dbContext.Completions.Add(completion);
        await dbContext.SaveChangesAsync();
        return completion.Id;
    }
}
