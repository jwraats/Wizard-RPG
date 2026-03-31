using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.CreatureTaming;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreatureTamingController : ControllerBase
{
    private readonly ICreatureTamingService _creatureTamingService;

    public CreatureTamingController(ICreatureTamingService creatureTamingService)
        => _creatureTamingService = creatureTamingService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all available creature types (catalog).</summary>
    [HttpGet("creatures")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CreatureResponse>>> GetCreatures()
    {
        var creatures = await _creatureTamingService.GetAllCreaturesAsync();
        return Ok(creatures);
    }

    /// <summary>Get all creatures tamed by the current player.</summary>
    [HttpGet("my-creatures")]
    public async Task<ActionResult<List<PlayerCreatureResponse>>> GetMyCreatures()
    {
        var creatures = await _creatureTamingService.GetPlayerCreaturesAsync(CurrentPlayerId);
        return Ok(creatures);
    }

    /// <summary>Explore the wilderness to discover a magical creature. Costs 50 gold.</summary>
    [HttpPost("explore")]
    public async Task<ActionResult<ExploreResponse>> Explore()
    {
        var result = await _creatureTamingService.ExploreAsync(CurrentPlayerId);
        return Ok(result);
    }

    /// <summary>Tame a discovered creature and add it to your collection.</summary>
    [HttpPost("tame")]
    public async Task<ActionResult<PlayerCreatureResponse>> Tame([FromBody] TameCreatureRequest request)
    {
        var creature = await _creatureTamingService.TameCreatureAsync(CurrentPlayerId, request);
        return Created($"api/creaturetaming/my-creatures", creature);
    }

    /// <summary>Care for a tamed creature: feed, train, or let it rest.</summary>
    [HttpPost("care/{creatureId:guid}")]
    public async Task<ActionResult<CareResponse>> CareForCreature(Guid creatureId, [FromBody] CareForCreatureRequest request)
    {
        var result = await _creatureTamingService.CareForCreatureAsync(CurrentPlayerId, creatureId, request);
        return Ok(result);
    }

    /// <summary>Get passive bonuses from all loyal creatures (loyalty > 50).</summary>
    [HttpGet("bonuses")]
    public async Task<ActionResult<Dictionary<string, int>>> GetBonuses()
    {
        var bonuses = await _creatureTamingService.GetCreatureBonusesAsync(CurrentPlayerId);
        return Ok(bonuses);
    }
}
