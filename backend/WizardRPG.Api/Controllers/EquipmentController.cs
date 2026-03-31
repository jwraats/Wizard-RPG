using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Equipment;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService) => _equipmentService = equipmentService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's equipment.</summary>
    [HttpGet]
    public async Task<ActionResult<EquipmentSlots>> GetMyEquipment()
    {
        var equipment = await _equipmentService.GetEquipmentAsync(CurrentPlayerId);
        return Ok(equipment);
    }

    /// <summary>Equip an item from the bank.</summary>
    [HttpPost("equip")]
    public async Task<ActionResult<EquipmentSlots>> EquipItem([FromBody] EquipItemRequest request)
    {
        var equipment = await _equipmentService.EquipItemAsync(CurrentPlayerId, request.BankItemId);
        return Ok(equipment);
    }

    /// <summary>Unequip an item from a slot.</summary>
    [HttpPost("unequip")]
    public async Task<ActionResult<EquipmentSlots>> UnequipItem([FromBody] UnequipItemRequest request)
    {
        var equipment = await _equipmentService.UnequipItemAsync(CurrentPlayerId, request.Slot);
        return Ok(equipment);
    }
}
