using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Player;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayerController(IPlayerService playerService) => _playerService = playerService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Player ID not found in token."));

    /// <summary>Get the current player's profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<PlayerProfileResponse>> GetMyProfile()
    {
        var profile = await _playerService.GetProfileAsync(CurrentPlayerId);
        return Ok(profile);
    }

    /// <summary>Get another player's profile by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerProfileResponse>> GetProfile(Guid id)
    {
        var profile = await _playerService.GetProfileAsync(id);
        return Ok(profile);
    }

    /// <summary>Update the current player's profile.</summary>
    [HttpPut("me")]
    public async Task<ActionResult<PlayerProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var profile = await _playerService.UpdateProfileAsync(CurrentPlayerId, request);
        return Ok(profile);
    }

    /// <summary>Get the top players leaderboard.</summary>
    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PlayerProfileResponse>>> GetLeaderboard([FromQuery] int top = 10)
    {
        var leaderboard = await _playerService.GetLeaderboardAsync(top);
        return Ok(leaderboard);
    }
}
