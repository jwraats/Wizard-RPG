namespace WizardRPG.Api.Models;

public class BattleTurn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BattleId { get; set; }
    public Guid AttackerId { get; set; }
    public Guid SpellId { get; set; }
    public int DamageDealt { get; set; }
    public int TurnNumber { get; set; }
    public string? Narrative { get; set; }

    public Battle? Battle { get; set; }
    public Player? Attacker { get; set; }
    public Spell? Spell { get; set; }
}
