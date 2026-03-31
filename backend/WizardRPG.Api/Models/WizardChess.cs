namespace WizardRPG.Api.Models;

public enum ChessMatchStatus
{
    Pending,
    Active,
    Finished,
    Forfeit
}

public class ChessMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChallengerId { get; set; }
    public Guid? DefenderId { get; set; }
    public Guid? WinnerId { get; set; }
    public ChessMatchStatus Status { get; set; } = ChessMatchStatus.Pending;
    public long BetAmount { get; set; } = 0;
    public string BoardState { get; set; } = string.Empty;
    public bool IsPlayerTurn { get; set; } = true;
    public int TurnCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public Player? Challenger { get; set; }
    public Player? Defender { get; set; }
    public Player? Winner { get; set; }
}
