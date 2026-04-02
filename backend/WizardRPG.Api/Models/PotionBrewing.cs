namespace WizardRPG.Api.Models;

public enum BrewResult
{
    Success,
    Failure,
    Explosion
}

public class PotionRecipe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1; // 1-5
    public int GoldReward { get; set; } = 0;
    public int XpReward { get; set; } = 0;

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<BrewAttempt> BrewAttempts { get; set; } = new List<BrewAttempt>();
}

public class PotionIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Price { get; set; } = 10;

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}

public class RecipeIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public Guid IngredientId { get; set; }
    public int Quantity { get; set; } = 1;

    public PotionRecipe? Recipe { get; set; }
    public PotionIngredient? Ingredient { get; set; }
}

public class BrewAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid RecipeId { get; set; }
    public BrewResult Result { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
    public PotionRecipe? Recipe { get; set; }
}
