namespace WizardRPG.Api.DTOs.LoginReward;

public record LoginRewardResponse(int Day, long GoldReward, string? ItemReward, int LoginStreak);
public record LoginRewardStatus(int LoginStreak, bool CanClaimToday, DateTime? LastClaimDate);
