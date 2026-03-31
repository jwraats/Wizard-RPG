using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Fellowship;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FellowshipController : ControllerBase
{
    private readonly IFellowshipService _fellowshipService;

    public FellowshipController(IFellowshipService fellowshipService) => _fellowshipService = fellowshipService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all fellowships.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<FellowshipResponse>>> GetAll()
    {
        var fellowships = await _fellowshipService.GetAllFellowshipsAsync();
        return Ok(fellowships);
    }

    /// <summary>Get a specific fellowship by ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<FellowshipResponse>> Get(Guid id)
    {
        var fellowship = await _fellowshipService.GetFellowshipAsync(id);
        return Ok(fellowship);
    }

    /// <summary>Get the current player's fellowship.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<FellowshipResponse?>> GetMine()
    {
        var fellowship = await _fellowshipService.GetPlayerFellowshipAsync(CurrentPlayerId);
        if (fellowship == null) return NoContent();
        return Ok(fellowship);
    }

    /// <summary>Create a new fellowship.</summary>
    [HttpPost]
    public async Task<ActionResult<FellowshipResponse>> Create([FromBody] CreateFellowshipRequest request)
    {
        var fellowship = await _fellowshipService.CreateFellowshipAsync(CurrentPlayerId, request);
        return Created($"api/fellowship/{fellowship.Id}", fellowship);
    }

    /// <summary>Join a fellowship using a referral code.</summary>
    [HttpPost("join")]
    public async Task<ActionResult<FellowshipResponse>> Join([FromBody] JoinFellowshipRequest request)
    {
        var fellowship = await _fellowshipService.JoinFellowshipAsync(CurrentPlayerId, request.ReferralCode);
        return Ok(fellowship);
    }

    /// <summary>Leave a fellowship.</summary>
    [HttpDelete("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        await _fellowshipService.LeaveFellowshipAsync(CurrentPlayerId, id);
        return NoContent();
    }

    /// <summary>Update a member's contribution percent (owner only).</summary>
    [HttpPut("{id:guid}/contribution")]
    public async Task<ActionResult<FellowshipResponse>> UpdateContribution(Guid id, [FromBody] UpdateContributionRequest request)
    {
        var fellowship = await _fellowshipService.UpdateContributionAsync(CurrentPlayerId, id, request);
        return Ok(fellowship);
    }
}
