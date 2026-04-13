using Cadence.API.Data;

namespace Cadence.API.Services.ArchiveHabitService;

public interface IArchiveHabitService
{
    public Task<long?> ExecuteAsync(long habitId);
}


public class ArchiveHabitService(AppDbContext dbContext) : IArchiveHabitService
{
    public async Task<long?> ExecuteAsync(long habitId)
    {
        var habit = await dbContext.Habits.FindAsync(habitId);

        if (habit == null)
            return null;

        habit.IsArchived = true;
        
        dbContext.Update(habit);
        
        await dbContext.SaveChangesAsync();
        
        return habit.Id;
    }
}