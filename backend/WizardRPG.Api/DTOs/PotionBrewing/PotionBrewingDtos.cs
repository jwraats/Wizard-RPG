using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.PotionBrewing;

public record PotionRecipeResponse(
    Guid Id,
    string Name,
    string Description,
    int Difficulty,
    int GoldReward,
    int XpReward,
    List<RecipeIngredientResponse> Ingredients);

public record RecipeIngredientResponse(Guid IngredientId, string IngredientName, int Quantity);

public record PotionIngredientResponse(Guid Id, string Name, string Description, long Price);

public record BrewAttemptResponse(Guid Id, Guid RecipeId, string RecipeName, BrewResult Result, DateTime CreatedAt);

public record BrewPotionRequest(Guid RecipeId);

public record BuyIngredientRequest(Guid IngredientId, int Quantity);
