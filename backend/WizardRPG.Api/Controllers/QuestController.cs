using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Quest;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/quest")]
[Authorize]
public class QuestController : ControllerBase
{
    private readonly IQuestService _questService;

    public QuestController(IQuestService questService) => _questService = questService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's quests.</summary>
    [HttpGet]
    public async Task<ActionResult<List<QuestResponse>>> GetMyQuests()
    {
        var quests = await _questService.GetPlayerQuestsAsync(CurrentPlayerId);
        return Ok(quests);
    }

    /// <summary>Generate daily quests for the current player.</summary>
    [HttpPost("generate-daily")]
    public async Task<IActionResult> GenerateDaily()
    {
        await _questService.GenerateDailyQuestsAsync(CurrentPlayerId);
        return Ok(new { message = "Daily quests generated." });
    }

    /// <summary>Generate weekly quests for the current player.</summary>
    [HttpPost("generate-weekly")]
    public async Task<IActionResult> GenerateWeekly()
    {
        await _questService.GenerateWeeklyQuestsAsync(CurrentPlayerId);
        return Ok(new { message = "Weekly quests generated." });
    }
}
