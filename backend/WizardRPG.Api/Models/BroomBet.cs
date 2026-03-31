namespace WizardRPG.Api.Models;

public enum BetStatus
{
    Pending,
    Won,
    Lost
}

public class BroomBet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid LeagueId { get; set; }
    public Guid TeamId { get; set; }
    public long Amount { get; set; }
    public BetStatus Status { get; set; } = BetStatus.Pending;
    public long Payout { get; set; } = 0;
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
    public BroomLeague? League { get; set; }
    public BroomTeam? Team { get; set; }
}
