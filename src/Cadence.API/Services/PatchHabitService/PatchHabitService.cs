using Cadence.API.Data;
using Cadence.API.Data.Entities;

namespace Cadence.API.Services.PatchHabitService;

public interface IPatchHabitService
{
    public Task<long?> ExecuteAsync(long habitId, PatchHabitDto input);
}

public class PatchHabitService(AppDbContext dbContext) : IPatchHabitService
{
    public async Task<long?> ExecuteAsync(long habitId,  PatchHabitDto input)
    {
        var habit = await dbContext.Habits.FindAsync(habitId);

        if (habit == null)
            return null;

        if (input.Name != null)
            habit.Name = input.Name;

        if (input.Color != null)
            habit.Color = input.Color;

        if (input.ScheduledDays != null)
            habit.ScheduledDays = input.ScheduledDays;

        if (input.Description != null)
            habit.Description = input.Description;

        if (input.TimeWindow != null)
        {
            habit.StartTime = input.TimeWindow.Start;
            habit.EndTime = input.TimeWindow.End;
        }
        
        dbContext.Update(habit);
        
        await dbContext.SaveChangesAsync();
        
        return habit.Id;
    }
}