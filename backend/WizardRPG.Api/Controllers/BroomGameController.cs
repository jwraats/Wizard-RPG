using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.BroomGame;
using WizardRPG.Api.Models;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BroomGameController : ControllerBase
{
    private readonly IBroomGameService _broomGameService;

    public BroomGameController(IBroomGameService broomGameService) => _broomGameService = broomGameService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all broom leagues, optionally filtered by status.</summary>
    [HttpGet("leagues")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BroomLeagueResponse>>> GetLeagues([FromQuery] LeagueStatus? status)
    {
        var leagues = await _broomGameService.GetLeaguesAsync(status);
        return Ok(leagues);
    }

    /// <summary>Get details of a specific league.</summary>
    [HttpGet("leagues/{leagueId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BroomLeagueResponse>> GetLeague(Guid leagueId)
    {
        var league = await _broomGameService.GetLeagueAsync(leagueId);
        return Ok(league);
    }

    /// <summary>Place a bet on a broom race.</summary>
    [HttpPost("bets")]
    public async Task<ActionResult<BroomBetResponse>> PlaceBet([FromBody] PlaceBetRequest request)
    {
        var bet = await _broomGameService.PlaceBetAsync(CurrentPlayerId, request);
        return Created($"api/broomgame/bets/{bet.Id}", bet);
    }

    /// <summary>Get all bets placed by the current player.</summary>
    [HttpGet("bets")]
    public async Task<ActionResult<List<BroomBetResponse>>> GetMyBets()
    {
        var bets = await _broomGameService.GetPlayerBetsAsync(CurrentPlayerId);
        return Ok(bets);
    }
}
