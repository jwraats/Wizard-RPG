using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.WhisperingWalls;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhisperingWallsController : ControllerBase
{
    private readonly IWhisperingWallsService _whisperingWallsService;

    public WhisperingWallsController(IWhisperingWallsService whisperingWallsService) =>
        _whisperingWallsService = whisperingWallsService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all available story arcs.</summary>
    [HttpGet("stories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<StoryArcResponse>>> GetStoryArcs()
    {
        var arcs = await _whisperingWallsService.GetStoryArcsAsync();
        return Ok(arcs);
    }

    /// <summary>Start or restart a story arc.</summary>
    [HttpPost("start")]
    public async Task<ActionResult<StoryChapterResponse>> StartStory([FromQuery] string storyArc)
    {
        var chapter = await _whisperingWallsService.StartStoryAsync(CurrentPlayerId, storyArc);
        return Ok(chapter);
    }

    /// <summary>Get the current chapter for a story arc.</summary>
    [HttpGet("current")]
    public async Task<ActionResult<StoryChapterResponse>> GetCurrentChapter([FromQuery] string storyArc)
    {
        var chapter = await _whisperingWallsService.GetCurrentChapterAsync(CurrentPlayerId, storyArc);
        return Ok(chapter);
    }

    /// <summary>Make a choice in the current chapter.</summary>
    [HttpPost("choose")]
    public async Task<ActionResult<MakeChoiceResponse>> MakeChoice([FromBody] MakeChoiceRequest request)
    {
        var result = await _whisperingWallsService.MakeChoiceAsync(CurrentPlayerId, request);
        return Ok(result);
    }

    /// <summary>Get all story progress for the current player.</summary>
    [HttpGet("progress")]
    public async Task<ActionResult<List<PlayerStoryProgressResponse>>> GetProgress()
    {
        var progress = await _whisperingWallsService.GetProgressAsync(CurrentPlayerId);
        return Ok(progress);
    }
}
