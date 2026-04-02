using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.DungeonCrawler;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DungeonCrawlerController : ControllerBase
{
    private readonly IDungeonCrawlerService _dungeonCrawlerService;

    public DungeonCrawlerController(IDungeonCrawlerService dungeonCrawlerService) => _dungeonCrawlerService = dungeonCrawlerService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Start a new dungeon run.</summary>
    [HttpPost("start")]
    public async Task<ActionResult<object>> StartRun()
    {
        var (run, room) = await _dungeonCrawlerService.StartRunAsync(CurrentPlayerId);
        return Created($"api/dungeoncrawler/active", new { run, room });
    }

    /// <summary>Get the active dungeon run and current room.</summary>
    [HttpGet("active")]
    public async Task<ActionResult<object>> GetActiveRun()
    {
        var result = await _dungeonCrawlerService.GetActiveRunAsync(CurrentPlayerId);
        if (result == null)
            return NotFound(new { message = "No active dungeon run." });

        var (run, room) = result.Value;
        return Ok(new { run, room });
    }

    /// <summary>Make a choice in the current room.</summary>
    [HttpPost("{runId:guid}/action")]
    public async Task<ActionResult<DungeonActionResponse>> MakeChoice(Guid runId, [FromBody] DungeonActionRequest request)
    {
        var response = await _dungeonCrawlerService.MakeChoiceAsync(CurrentPlayerId, runId, request);
        return Ok(response);
    }

    /// <summary>Escape the dungeon with your collected loot.</summary>
    [HttpPost("{runId:guid}/escape")]
    public async Task<ActionResult<DungeonRunResponse>> Escape(Guid runId)
    {
        var run = await _dungeonCrawlerService.EscapeAsync(CurrentPlayerId, runId);
        return Ok(run);
    }

    /// <summary>Get dungeon run history.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<DungeonRunResponse>>> GetHistory()
    {
        var runs = await _dungeonCrawlerService.GetRunHistoryAsync(CurrentPlayerId);
        return Ok(runs);
    }
}
