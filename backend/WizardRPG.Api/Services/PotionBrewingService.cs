using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.PotionBrewing;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IPotionBrewingService
{
    Task<List<PotionRecipeResponse>> GetRecipesAsync();
    Task<List<PotionIngredientResponse>> GetIngredientsAsync();
    Task<BrewAttemptResponse> BrewPotionAsync(Guid playerId, BrewPotionRequest request);
    Task<List<BrewAttemptResponse>> GetPlayerBrewHistoryAsync(Guid playerId);
}

public class PotionBrewingService : IPotionBrewingService
{
    private readonly AppDbContext _db;

    public PotionBrewingService(AppDbContext db) => _db = db;

    public async Task<List<PotionRecipeResponse>> GetRecipesAsync()
    {
        var recipes = await _db.PotionRecipes
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .OrderBy(r => r.Difficulty)
            .ToListAsync();

        return recipes.Select(MapRecipeToResponse).ToList();
    }

    public async Task<List<PotionIngredientResponse>> GetIngredientsAsync()
    {
        var ingredients = await _db.PotionIngredients
            .OrderBy(i => i.Name)
            .ToListAsync();

        return ingredients.Select(i => new PotionIngredientResponse(
            i.Id, i.Name, i.Description, i.Price)).ToList();
    }

    public async Task<BrewAttemptResponse> BrewPotionAsync(Guid playerId, BrewPotionRequest request)
    {
        var recipe = await _db.PotionRecipes
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId)
            ?? throw new KeyNotFoundException("Recipe not found.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        // Calculate cost = sum of all ingredient prices * quantities
        long cost = recipe.Ingredients.Sum(ri => ri.Ingredient!.Price * ri.Quantity);

        if (player.GoldCoins < cost)
            throw new InvalidOperationException("Insufficient gold coins to purchase ingredients.");

        player.GoldCoins -= cost;

        // Determine brew result
        int successChance = Math.Min(90, 50 + player.Wisdom - recipe.Difficulty * 8);
        int roll = Random.Shared.Next(1, 101);

        BrewResult result;
        if (roll <= successChance)
        {
            result = BrewResult.Success;
            player.GoldCoins += recipe.GoldReward;
            player.Experience += recipe.XpReward;
        }
        else if (roll > 95)
        {
            result = BrewResult.Explosion;
        }
        else
        {
            result = BrewResult.Failure;
        }

        var attempt = new BrewAttempt
        {
            PlayerId = playerId,
            RecipeId = recipe.Id,
            Result = result
        };

        _db.BrewAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        return new BrewAttemptResponse(
            attempt.Id, recipe.Id, recipe.Name, attempt.Result, attempt.CreatedAt);
    }

    public async Task<List<BrewAttemptResponse>> GetPlayerBrewHistoryAsync(Guid playerId)
    {
        var attempts = await _db.BrewAttempts
            .Include(a => a.Recipe)
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return attempts.Select(a => new BrewAttemptResponse(
            a.Id, a.RecipeId, a.Recipe!.Name, a.Result, a.CreatedAt)).ToList();
    }

    private static PotionRecipeResponse MapRecipeToResponse(PotionRecipe r) => new(
        r.Id, r.Name, r.Description, r.Difficulty, r.GoldReward, r.XpReward,
        r.Ingredients.Select(ri => new RecipeIngredientResponse(
            ri.IngredientId, ri.Ingredient!.Name, ri.Quantity)).ToList());
}
