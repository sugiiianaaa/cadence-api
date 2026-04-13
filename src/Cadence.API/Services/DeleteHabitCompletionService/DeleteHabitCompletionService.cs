using Cadence.API.Data;
using Cadence.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.DeleteHabitCompletionService;

public interface IDeleteHabitCompletionService
{
    public Task<long?> ExecuteAsync(long habitId);
}

public class DeleteHabitCompletionService(AppDbContext dbContext) : IDeleteHabitCompletionService
{
    public async Task<long?> ExecuteAsync(long habitId)
    {
        var habitExists = await dbContext.Habits.AnyAsync(h => h.Id == habitId);

        if (!habitExists)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var completion = await dbContext.Completions
            .Where(c => c.Date == today)
            .FirstOrDefaultAsync(c => c.HabitId == habitId);

        if (completion == null)
            return null;
        
        dbContext.Completions.Remove(completion);
        await dbContext.SaveChangesAsync();
        return completion.Id;
    }
}