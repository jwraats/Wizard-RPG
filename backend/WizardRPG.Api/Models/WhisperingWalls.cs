namespace WizardRPG.Api.Models;

public class StoryChapter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string StoryArc { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;
    public bool IsEnding { get; set; } = false;
    public int? GoldReward { get; set; }
    public int? XpReward { get; set; }

    public ICollection<StoryChoice> Choices { get; set; } = new List<StoryChoice>();
}

public class StoryChoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChapterId { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public Guid? NextChapterId { get; set; }
    public string? RequiredItemName { get; set; }
    public int? MinWisdom { get; set; }

    public StoryChapter? Chapter { get; set; }
    public StoryChapter? NextChapter { get; set; }
}

public class PlayerStoryProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid CurrentChapterId { get; set; }
    public string StoryArc { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Player? Player { get; set; }
    public StoryChapter? CurrentChapter { get; set; }
}
