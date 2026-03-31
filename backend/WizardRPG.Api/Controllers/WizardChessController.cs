using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.WizardChess;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WizardChessController : ControllerBase
{
    private readonly IWizardChessService _chessService;

    public WizardChessController(IWizardChessService chessService) => _chessService = chessService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Create a new chess match.</summary>
    [HttpPost("create")]
    public async Task<ActionResult<ChessMatchResponse>> CreateMatch([FromBody] CreateChessMatchRequest request)
    {
        var match = await _chessService.CreateMatchAsync(CurrentPlayerId, request);
        return Created($"api/wizardchess/{match.Id}", match);
    }

    /// <summary>Get a specific chess match.</summary>
    [HttpGet("{matchId:guid}")]
    public async Task<ActionResult<ChessMatchResponse>> GetMatch(Guid matchId)
    {
        var match = await _chessService.GetMatchAsync(matchId);
        return Ok(match);
    }

    /// <summary>Get all matches for the current player.</summary>
    [HttpGet("matches")]
    public async Task<ActionResult<List<ChessMatchResponse>>> GetMyMatches()
    {
        var matches = await _chessService.GetPlayerMatchesAsync(CurrentPlayerId);
        return Ok(matches);
    }

    /// <summary>Make a move in a chess match.</summary>
    [HttpPost("{matchId:guid}/move")]
    public async Task<ActionResult<ChessMoveResponse>> MakeMove(Guid matchId, [FromBody] ChessMoveRequest request)
    {
        var result = await _chessService.MakeMoveAsync(CurrentPlayerId, matchId, request);
        return Ok(result);
    }

    /// <summary>Forfeit a chess match.</summary>
    [HttpPost("{matchId:guid}/forfeit")]
    public async Task<ActionResult<ChessMatchResponse>> Forfeit(Guid matchId)
    {
        var match = await _chessService.ForfeitAsync(CurrentPlayerId, matchId);
        return Ok(match);
    }
}
