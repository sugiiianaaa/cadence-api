using Cadence.API.Data;
using Cadence.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cadence.API.Services.UpdateCompletionService;

public interface IUpdateCompletionService
{
    public Task ExecuteAsync(long habitId);
}

public class UpdateCompletionService(AppDbContext dbContext) : IUpdateCompletionService
{
    public async Task ExecuteAsync(long habitId)
    {
        var completion = await dbContext.Completions
            .Where(c => c.HabitId == habitId && c.Date == DateOnly.FromDateTime(DateTime.Today))
            .FirstOrDefaultAsync();

        if (completion is not null)
        {
            dbContext.Completions.Remove(completion);
        }
        else
        {
            completion = new Completion
            {
                HabitId = habitId,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };
            
            dbContext.Completions.Add(completion);
        }
        
        await dbContext.SaveChangesAsync();
        
    }
}