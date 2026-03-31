using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WizardRPG.Api.DTOs.Battle;
using WizardRPG.Api.Hubs;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BattleController : ControllerBase
{
    private readonly IBattleService _battleService;
    private readonly IHubContext<BattleHub> _battleHub;

    public BattleController(IBattleService battleService, IHubContext<BattleHub> battleHub)
    {
        _battleService = battleService;
        _battleHub = battleHub;
    }

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all available spells.</summary>
    [HttpGet("spells")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SpellResponse>>> GetSpells()
    {
        var spells = await _battleService.GetSpellsAsync();
        return Ok(spells);
    }

    /// <summary>Challenge another player to a battle.</summary>
    [HttpPost("challenge")]
    public async Task<ActionResult<BattleResponse>> Challenge([FromBody] ChallengeBattleRequest request)
    {
        var battle = await _battleService.ChallengeBattleAsync(CurrentPlayerId, request.DefenderId);
        await _battleHub.Clients.User(request.DefenderId.ToString())
            .SendAsync("BattleChallenge", battle);
        return Created($"api/battle/{battle.Id}", battle);
    }

    /// <summary>Accept a battle challenge.</summary>
    [HttpPost("{battleId:guid}/accept")]
    public async Task<ActionResult<BattleResponse>> Accept(Guid battleId)
    {
        var battle = await _battleService.AcceptBattleAsync(battleId, CurrentPlayerId);
        await _battleHub.Clients.Group($"battle-{battleId}")
            .SendAsync("BattleStarted", battle);
        return Ok(battle);
    }

    /// <summary>Get a specific battle.</summary>
    [HttpGet("{battleId:guid}")]
    public async Task<ActionResult<BattleResponse>> GetBattle(Guid battleId)
    {
        var battle = await _battleService.GetBattleAsync(battleId);
        return Ok(battle);
    }

    /// <summary>Get the current player's battles.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<BattleResponse>>> GetMyBattles()
    {
        var battles = await _battleService.GetPlayerBattlesAsync(CurrentPlayerId);
        return Ok(battles);
    }

    /// <summary>Cast a spell in a battle (execute a turn).</summary>
    [HttpPost("{battleId:guid}/cast")]
    public async Task<ActionResult<BattleResponse>> CastSpell(Guid battleId, [FromBody] CastSpellRequest request)
    {
        var battle = await _battleService.ExecuteTurnAsync(battleId, CurrentPlayerId, request.SpellId);
        await _battleHub.Clients.Group($"battle-{battleId}")
            .SendAsync("TurnExecuted", battle);
        return Ok(battle);
    }
}
