namespace WizardRPG.Api.DTOs.WhisperingWalls;

public record StoryChapterResponse(
    Guid Id, string Title, string Content, string StoryArc,
    bool IsEnding, int? GoldReward, int? XpReward,
    List<StoryChoiceResponse> Choices);

public record StoryChoiceResponse(
    Guid Id, string ChoiceText, bool IsAvailable, string? RequirementHint);

public record StoryArcResponse(string StoryArc, string FirstChapterTitle, int TotalChapters);

public record MakeChoiceRequest(Guid ChoiceId);

public record MakeChoiceResponse(
    string Narrative, bool IsEnding, int? GoldEarned, int? XpEarned,
    StoryChapterResponse? NextChapter);

public record PlayerStoryProgressResponse(
    Guid Id, string StoryArc, Guid CurrentChapterId,
    string CurrentChapterTitle, bool IsCompleted,
    DateTime StartedAt, DateTime? CompletedAt);
