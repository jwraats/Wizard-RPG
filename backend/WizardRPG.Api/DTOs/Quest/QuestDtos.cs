namespace WizardRPG.Api.DTOs.Quest;

using WizardRPG.Api.Models;

public record QuestResponse(Guid Id, string Title, string Description, QuestType Type, QuestStatus Status, int TargetCount, int CurrentCount, long GoldReward, int XpReward, DateTime ExpiresAt);
