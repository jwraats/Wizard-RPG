using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Achievement;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/achievement")]
[Authorize]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementController(IAchievementService achievementService) => _achievementService = achievementService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's achievements.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AchievementResponse>>> GetMyAchievements()
    {
        var achievements = await _achievementService.GetPlayerAchievementsAsync(CurrentPlayerId);
        return Ok(achievements);
    }

    /// <summary>Check and award achievements for the current player.</summary>
    [HttpPost("check")]
    public async Task<IActionResult> CheckAchievements()
    {
        await _achievementService.CheckAndAwardAchievementsAsync(CurrentPlayerId);
        return Ok(new { message = "Achievements checked." });
    }
}
