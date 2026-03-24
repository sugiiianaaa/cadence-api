using Cadence.API.Services.CreateHabitsService;
using Cadence.API.Services.GetHabitsService;
using Cadence.API.Services.UpdateCompletionService;
using Microsoft.AspNetCore.Mvc;

namespace Cadence.API.Controllers;

[ApiController]
[Route("habit")]
public class HabitController(IGetHabitsService getHabitsService, ICreateHabitService createHabitService, IUpdateCompletionService updateCompletionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabitsAsync()
    {
        var result = await getHabitsService.ExecuteAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHabitAsync(CreateHabitInputDto input)
    {
        var newHabitId = await createHabitService.ExecuteAsync(input);
        return Created("",newHabitId);
    }

    [HttpPost("completion/{habitId:long}")]
    public async Task<IActionResult> CompleteHabitAsync(long habitId)
    {
        await updateCompletionService.ExecuteAsync(habitId);
        return Ok();
    }
}