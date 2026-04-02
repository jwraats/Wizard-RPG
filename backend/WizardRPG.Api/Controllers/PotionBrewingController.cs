using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.PotionBrewing;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PotionBrewingController : ControllerBase
{
    private readonly IPotionBrewingService _potionBrewingService;

    public PotionBrewingController(IPotionBrewingService potionBrewingService) => _potionBrewingService = potionBrewingService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get all potion recipes.</summary>
    [HttpGet("recipes")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PotionRecipeResponse>>> GetRecipes()
    {
        var recipes = await _potionBrewingService.GetRecipesAsync();
        return Ok(recipes);
    }

    /// <summary>Get all available ingredients.</summary>
    [HttpGet("ingredients")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PotionIngredientResponse>>> GetIngredients()
    {
        var ingredients = await _potionBrewingService.GetIngredientsAsync();
        return Ok(ingredients);
    }

    /// <summary>Attempt to brew a potion.</summary>
    [HttpPost("brew")]
    public async Task<ActionResult<BrewAttemptResponse>> BrewPotion([FromBody] BrewPotionRequest request)
    {
        var result = await _potionBrewingService.BrewPotionAsync(CurrentPlayerId, request);
        return Created($"api/potionbrewing/history/{result.Id}", result);
    }

    /// <summary>Get the current player's brew history.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<BrewAttemptResponse>>> GetBrewHistory()
    {
        var history = await _potionBrewingService.GetPlayerBrewHistoryAsync(CurrentPlayerId);
        return Ok(history);
    }
}
