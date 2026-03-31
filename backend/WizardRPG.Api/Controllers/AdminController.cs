using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Admin;
using WizardRPG.Api.DTOs.BroomGame;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IBroomGameService _broomGameService;

    public AdminController(IAdminService adminService, IBroomGameService broomGameService)
    {
        _adminService = adminService;
        _broomGameService = broomGameService;
    }

    private bool IsAdmin => User.FindFirstValue("isAdmin") == "true";

    private void RequireAdmin()
    {
        if (!IsAdmin)
            throw new UnauthorizedAccessException("Admin access required.");
    }

    // ─── Players ─────────────────────────────────────────────

    /// <summary>Get all players (Admin only).</summary>
    [HttpGet("players")]
    public async Task<ActionResult<List<AdminPlayerResponse>>> GetPlayers()
    {
        RequireAdmin();
        return Ok(await _adminService.GetAllPlayersAsync());
    }

    /// <summary>Get a specific player (Admin only).</summary>
    [HttpGet("players/{playerId:guid}")]
    public async Task<ActionResult<AdminPlayerResponse>> GetPlayer(Guid playerId)
    {
        RequireAdmin();
        return Ok(await _adminService.GetPlayerAsync(playerId));
    }

    /// <summary>Set a player's gold (Admin only).</summary>
    [HttpPut("players/gold")]
    public async Task<ActionResult<AdminPlayerResponse>> SetGold([FromBody] SetGoldRequest request)
    {
        RequireAdmin();
        return Ok(await _adminService.SetGoldAsync(request));
    }

    /// <summary>Grant or revoke admin status (Admin only).</summary>
    [HttpPut("players/admin-status")]
    public async Task<ActionResult<AdminPlayerResponse>> SetAdminStatus([FromBody] SetAdminRequest request)
    {
        RequireAdmin();
        return Ok(await _adminService.SetAdminStatusAsync(request));
    }

    // ─── Items ───────────────────────────────────────────────

    /// <summary>Create a new item (Admin only).</summary>
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest request)
    {
        RequireAdmin();
        var item = await _adminService.CreateItemAsync(request);
        return Created($"api/items/{item.Id}", item);
    }

    /// <summary>Delete an item (Admin only).</summary>
    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid itemId)
    {
        RequireAdmin();
        await _adminService.DeleteItemAsync(itemId);
        return NoContent();
    }

    /// <summary>Grant an item to a player (Admin only).</summary>
    [HttpPost("items/grant")]
    public async Task<IActionResult> GrantItem([FromBody] GrantItemRequest request)
    {
        RequireAdmin();
        await _adminService.GrantItemAsync(request);
        return Ok();
    }

    /// <summary>Revoke an item from a player (Admin only).</summary>
    [HttpDelete("items/revoke")]
    public async Task<IActionResult> RevokeItem([FromBody] RevokeItemRequest request)
    {
        RequireAdmin();
        await _adminService.RevokeItemAsync(request);
        return NoContent();
    }

    // ─── Broom Game ─────────────────────────────────────────

    /// <summary>Create a new broom league (Admin only).</summary>
    [HttpPost("leagues")]
    public async Task<ActionResult<BroomLeagueResponse>> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        RequireAdmin();
        var league = await _broomGameService.CreateLeagueAsync(request);
        return Created($"api/broomgame/leagues/{league.Id}", league);
    }

    /// <summary>Resolve a broom league and pay out winners (Admin only).</summary>
    [HttpPost("leagues/{leagueId:guid}/resolve")]
    public async Task<ActionResult<BroomLeagueResponse>> ResolveLeague(Guid leagueId, [FromBody] ResolveLeagueRequest request)
    {
        RequireAdmin();
        var league = await _broomGameService.ResolveLeagueAsync(leagueId, request.WinnerTeamId);
        return Ok(league);
    }
}
