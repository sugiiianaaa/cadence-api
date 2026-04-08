using Cadence.API.Services.CreateHabitsService;
using Cadence.API.Services.GetHabitByIdService;
using Cadence.API.Services.GetHabitsService;
using Cadence.API.Services.GetHabitStatusService;
using Cadence.API.Services.GetTodayHabitsService;
using Cadence.API.Services.UpdateHabitCompletionService;
using Microsoft.AspNetCore.Mvc;

namespace Cadence.API.Controllers;

[ApiController]
[Route("api/habits")]
public class HabitController(
    IGetHabitsService getHabitsService,
    ICreateHabitService createHabitService,
    IUpdateHabitCompletionService updateHabitCompletionService,
    IGetHabitByIdService getHabitByIdService,
    IGetHabitStatusService getHabitStatusService,
    IGetTodayHabitsService getTodayHabitsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GetHabitOutputDto>>> GetHabitsAsync()
    {
        var result = await getHabitsService.ExecuteAsync();
        return Ok(result);
    }

    [HttpGet("{habitId:long}")]
    public async Task<ActionResult<GetHabitByIdOutputDto>> GetHabitById(long habitId)
    {
        var result = await getHabitByIdService.ExecuteAsync(habitId);
        
        return result == null 
            ? Problem(detail: $"Habit {habitId} not found.", statusCode: 404) 
            : Ok(result);
    }

    [HttpGet("today")]
    public async Task<ActionResult<GetTodayHabitsDto>> GetTodayHabits()
    {
        var result = await getTodayHabitsService.ExecuteAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<long>> CreateHabit(CreateHabitInputDto input)
    {
        var newHabitId = await createHabitService.ExecuteAsync(input);
        return CreatedAtAction("GetHabitById", new { habitId = newHabitId }, newHabitId);
    }

    [HttpPost("{habitId:long}/completion")]
    public async Task<ActionResult> ToggleCompletion(long habitId)
    {
        var found = await updateHabitCompletionService.ExecuteAsync(habitId);
        
        if (!found) 
            return Problem(detail: $"Habit {habitId} not found.", statusCode: 404);
        
        return Ok();
    }

    [HttpGet("status")]
    public async Task<ActionResult<GetHabitStatusOutput>> GetHabitStatus()
    {
        var result  = await getHabitStatusService.ExecuteAsync();
        return Ok(result);
    }
}