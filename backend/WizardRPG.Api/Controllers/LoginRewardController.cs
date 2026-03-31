using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.LoginReward;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/login-reward")]
[Authorize]
public class LoginRewardController : ControllerBase
{
    private readonly ILoginRewardService _loginRewardService;

    public LoginRewardController(ILoginRewardService loginRewardService) => _loginRewardService = loginRewardService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's login reward status.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<LoginRewardStatus>> GetStatus()
    {
        var status = await _loginRewardService.GetLoginRewardStatusAsync(CurrentPlayerId);
        return Ok(status);
    }

    /// <summary>Claim the daily login reward.</summary>
    [HttpPost("claim")]
    public async Task<ActionResult<LoginRewardResponse>> ClaimReward()
    {
        var reward = await _loginRewardService.ClaimDailyRewardAsync(CurrentPlayerId);
        return Ok(reward);
    }
}
