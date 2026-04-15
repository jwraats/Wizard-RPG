namespace WizardRPG.Api.DTOs.Achievement;

public record AchievementResponse(Guid Id, string Key, string Name, string Description, DateTime UnlockedAt);
