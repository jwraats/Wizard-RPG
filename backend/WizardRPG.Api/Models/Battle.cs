namespace WizardRPG.Api.Models;

public enum BattleStatus
{
    Pending,
    Active,
    Finished
}

public class Battle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChallengerId { get; set; }
    public Guid DefenderId { get; set; }
    public BattleStatus Status { get; set; } = BattleStatus.Pending;
    public Guid? WinnerId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string? NarratorStory { get; set; }

    public Player? Challenger { get; set; }
    public Player? Defender { get; set; }
    public Player? Winner { get; set; }
    public ICollection<BattleTurn> Turns { get; set; } = new List<BattleTurn>();
}
