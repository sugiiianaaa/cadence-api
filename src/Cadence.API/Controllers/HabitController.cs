using Cadence.API.Services.ArchiveHabitService;
using Cadence.API.Services.CreateHabitCompletionService;
using Cadence.API.Services.CreateHabitsService;
using Cadence.API.Services.DeleteHabitCompletionService;
using Cadence.API.Services.GetHabitByIdService;
using Cadence.API.Services.GetHabitsService;
using Cadence.API.Services.GetTodayHabitsService;
using Cadence.API.Services.PatchHabitService;
using Microsoft.AspNetCore.Mvc;

namespace Cadence.API.Controllers;

[ApiController]
[Route("api/habits")]
public class HabitController(
    IGetHabitsService getHabitsService,
    ICreateHabitService createHabitService,
    IGetHabitByIdService getHabitByIdService,
    IGetTodayHabitsService getTodayHabitsService,
    IPatchHabitService patchHabitService,
    IArchiveHabitService archiveHabitService,
    ICreateHabitCompletionService createHabitCompletionService,
    IDeleteHabitCompletionService deleteHabitCompletionService) : ControllerBase
{
    /// <summary>
    /// Get all unarchived habits.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GetHabitOutputDto>>> GetHabitsAsync()
    {
        var result = await getHabitsService.ExecuteAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get single habit based on habitId
    /// </summary>
    /// <param name="habitId">long</param>
    [HttpGet("{habitId:long}")]
    public async Task<ActionResult<GetHabitByIdOutputDto>> GetHabitById(long habitId)
    {
        var result = await getHabitByIdService.ExecuteAsync(habitId);

        return result == null
            ? Problem(detail: $"Habit {habitId} not found.", statusCode: 404)
            : Ok(result);
    }

    /// <summary>
    /// Get all habits that should be processed on DateOnly.FromDateTime(DateTime.UtcNow).
    /// </summary>
    [HttpGet("today")]
    public async Task<ActionResult<GetTodayHabitsDto>> GetTodayHabits()
    {
        var result = await getTodayHabitsService.ExecuteAsync();
        return Ok(result);
    }

    /// <summary>
    /// Add new habit record.
    /// </summary>
    /// <param name="input">CreateHabitInputDto</param>
    [HttpPost]
    public async Task<ActionResult<long>> CreateHabit(CreateHabitInputDto input)
    {
        var newHabitId = await createHabitService.ExecuteAsync(input);
        return CreatedAtAction("GetHabitById", new { habitId = newHabitId }, newHabitId);
    }

    /// <summary>
    /// Update existing habit metadata
    /// </summary>
    /// <returns></returns>
    [HttpPatch("{habitId:long}")]
    public async Task<ActionResult<long>> PatchHabit(long habitId, PatchHabitDto input)
    {
        var updatedHabitId = await patchHabitService.ExecuteAsync(habitId, input);
        
        if (updatedHabitId == null)
            return NotFound();
        
        return Ok(updatedHabitId);
    }
    
    /// <summary>
    /// Archive a habit
    /// </summary>
    /// <param name="habitId">long</param>
    [HttpDelete("{habitId:long}")]
    public async Task<ActionResult<long>> ArchiveHabit(long habitId)
    {
        var archivedHabitId = await archiveHabitService.ExecuteAsync(habitId);

        if (archivedHabitId == null)
            return NotFound();
        
        return Ok(archivedHabitId);
    }

    /// <summary>
    /// Add completion to a habit
    /// </summary>
    /// <param name="habitId">long</param>
    [HttpPost("{habitId:long}/completion")]
    public async Task<ActionResult<long>> CreateHabitCompletion(long habitId)
    {
        var newCompletionId = await createHabitCompletionService.ExecuteAsync(habitId);
        
        if (newCompletionId == null)
            return NotFound();
        
        return Ok(newCompletionId);
    }
    
    /// <summary>
    /// Delete completion from a habit
    /// </summary>
    /// <param name="habitId">long</param>
    [HttpDelete("{habitId:long}/completion")]
    public async Task<ActionResult<long>> DeleteHabitCompletion(long habitId)
    {
        var deletedCompletionId = await deleteHabitCompletionService.ExecuteAsync(habitId);
        
        if (deletedCompletionId == null)
            return NotFound();
        
        return Ok(deletedCompletionId);
    }
}