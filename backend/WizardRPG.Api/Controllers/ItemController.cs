using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly IAdminService _adminService;

    public ItemController(IAdminService adminService) => _adminService = adminService;

    /// <summary>Get all available items in the game.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetItems()
    {
        var items = await _adminService.GetAllItemsAsync();
        return Ok(items);
    }
}
