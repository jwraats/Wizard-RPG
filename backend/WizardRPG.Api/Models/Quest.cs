namespace WizardRPG.Api.Models;

public enum QuestType
{
    Daily,
    Weekly
}

public enum QuestStatus
{
    Active,
    Completed,
    Expired
}

public class Quest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType Type { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Active;
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; } = 0;
    public long GoldReward { get; set; }
    public int XpReward { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public Player? Player { get; set; }
}
