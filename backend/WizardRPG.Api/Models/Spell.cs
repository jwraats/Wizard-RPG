namespace WizardRPG.Api.Models;

public enum SpellElement
{
    Fire,
    Ice,
    Lightning,
    Earth,
    Arcane
}

public class Spell
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ManaCost { get; set; }
    public int BaseDamage { get; set; }
    public string Effect { get; set; } = string.Empty;
    public SpellElement Element { get; set; }

    public ICollection<BattleTurn> BattleTurns { get; set; } = new List<BattleTurn>();
}
