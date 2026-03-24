using Cadence.API.Data;
using Cadence.API.Data.Entities;

namespace Cadence.API.Services.CreateHabitsService;

public interface ICreateHabitService
{
    public Task<long> ExecuteAsync(CreateHabitInputDto input);
}

public class CreateHabitService(AppDbContext dbContext) : ICreateHabitService
{
    public async Task<long> ExecuteAsync(CreateHabitInputDto input)
    {
        var habitEntity = new Habit
        {
            Name = input.Name,
            Description = input.Description,
            Color = input.Color,
            ScheduledDays = input.ScheduledDays.ToArray(),
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            Completions = [],
        };
        
        await dbContext.AddAsync(habitEntity);
        await dbContext.SaveChangesAsync();
        
        return habitEntity.Id;
    }
}