using Cadence.API.Data;
using Cadence.API.Data.Entities;

namespace Cadence.API.Services.UpdateCompletionService;

public interface IUpdateCompletionService
{
    public Task ExecuteAsync(long habitId);
}

public class UpdateCompletionService(AppDbContext dbContext) : IUpdateCompletionService
{
    public async Task ExecuteAsync(long habitId)
    {
        var habit = await dbContext.Habits.FindAsync(habitId);
        
        if (habit == null)
            throw new InvalidOperationException("Habit not found");

        var completion = habit.Completions?.FirstOrDefault(c => c.Date == DateOnly.FromDateTime(DateTime.Today));
        
        if(completion == null)
            habit.Completions?.Add(new Completion{Date = DateOnly.FromDateTime(DateTime.Today), HabitId = habitId});
        else
            habit.Completions!.Remove(completion);
        
        dbContext.Update(habit);
        
        await dbContext.SaveChangesAsync();
    }
}