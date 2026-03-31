namespace WizardRPG.Api.Models;

public enum CreatureRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public class Creature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CreatureRarity Rarity { get; set; }
    public int BaseHealth { get; set; } = 50;
    public int BaseAttack { get; set; } = 10;
    public string BonusType { get; set; } = string.Empty; // "gold", "magic", "strength", "wisdom", "speed"
    public int BonusValue { get; set; } = 0;

    public ICollection<PlayerCreature> PlayerCreatures { get; set; } = new List<PlayerCreature>();
}

public class PlayerCreature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid CreatureId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Happiness { get; set; } = 50; // 0-100
    public int Loyalty { get; set; } = 0; // 0-100
    public int Level { get; set; } = 1;
    public DateTime? LastFedAt { get; set; }
    public DateTime? LastTrainedAt { get; set; }
    public DateTime TamedAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
    public Creature? Creature { get; set; }
}
