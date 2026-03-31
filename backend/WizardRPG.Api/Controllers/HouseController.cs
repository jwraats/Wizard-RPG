using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.House;
using WizardRPG.Api.DTOs.Player;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/house")]
[Authorize]
public class HouseController : ControllerBase
{
    private readonly IHouseService _houseService;

    public HouseController(IHouseService houseService) => _houseService = houseService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the house leaderboard.</summary>
    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult<List<HouseLeaderboardEntry>>> GetLeaderboard()
    {
        var leaderboard = await _houseService.GetHouseLeaderboardAsync();
        return Ok(leaderboard);
    }

    /// <summary>Get house points for a specific house.</summary>
    [HttpGet("{house}/points")]
    public async Task<ActionResult<List<HousePointsResponse>>> GetHousePoints(string house, [FromQuery] int limit = 50)
    {
        var points = await _houseService.GetHousePointsAsync(house, limit);
        return Ok(points);
    }

    /// <summary>Select a house for the current player.</summary>
    [HttpPost("select")]
    public async Task<ActionResult<PlayerProfileResponse>> SelectHouse([FromBody] SelectHouseRequest request)
    {
        var profile = await _houseService.SelectHouseAsync(CurrentPlayerId, request.House);
        return Ok(profile);
    }

    /// <summary>Award house points (admin only).</summary>
    [HttpPost("points")]
    public async Task<ActionResult<HousePointsResponse>> AwardPoints([FromBody] AwardHousePointsRequest request)
    {
        // Check admin
        var isAdmin = User.FindFirstValue("isAdmin");
        if (isAdmin != "true")
            return Forbid();

        var result = await _houseService.AwardHousePointsAsync(request.PlayerId, request.Points, request.Activity);
        return Ok(result);
    }
}
