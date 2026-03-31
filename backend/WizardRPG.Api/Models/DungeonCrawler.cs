namespace WizardRPG.Api.Models;

public enum DungeonRunStatus
{
    Active,
    Escaped,
    Defeated
}

public enum RoomType
{
    Monster,
    Treasure,
    Trap,
    Merchant,
    Rest,
    Boss
}

public class DungeonRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public int CurrentFloor { get; set; } = 1;
    public int CurrentHp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public long GoldCollected { get; set; } = 0;
    public int XpCollected { get; set; } = 0;
    public DungeonRunStatus Status { get; set; } = DungeonRunStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public Player? Player { get; set; }
}
