using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.LoginReward;

namespace WizardRPG.Api.Services;

public interface ILoginRewardService
{
    Task<LoginRewardResponse> ClaimDailyRewardAsync(Guid playerId);
    Task<LoginRewardStatus> GetLoginRewardStatusAsync(Guid playerId);
}

public class LoginRewardService : ILoginRewardService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public LoginRewardService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<LoginRewardStatus> GetLoginRewardStatusAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var canClaim = player.LastLoginRewardDate == null ||
                       player.LastLoginRewardDate.Value.Date < DateTime.UtcNow.Date;

        return new LoginRewardStatus(player.LoginStreak, canClaim, player.LastLoginRewardDate);
    }

    public async Task<LoginRewardResponse> ClaimDailyRewardAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var today = DateTime.UtcNow.Date;

        if (player.LastLoginRewardDate != null && player.LastLoginRewardDate.Value.Date >= today)
            throw new InvalidOperationException("Daily reward already claimed today.");

        // Update streak
        if (player.LastLoginRewardDate != null && player.LastLoginRewardDate.Value.Date == today.AddDays(-1))
        {
            player.LoginStreak++;
        }
        else
        {
            player.LoginStreak = 1;
        }

        player.LastLoginRewardDate = DateTime.UtcNow;
        player.LastLoginDate = DateTime.UtcNow;

        var day = player.LoginStreak;
        var (goldReward, itemReward) = GetRewardForDay(day);

        player.GoldCoins += goldReward;

        await _db.SaveChangesAsync();

        var rewardMessage = goldReward > 0 ? $"{goldReward} gold" : "";
        if (itemReward != null)
            rewardMessage = string.IsNullOrEmpty(rewardMessage) ? itemReward : $"{rewardMessage} + {itemReward}";

        await _notifications.CreateNotificationAsync(playerId,
            "Daily Reward!",
            $"Day {day} reward: {rewardMessage}",
            "login_reward");

        return new LoginRewardResponse(day, goldReward, itemReward, player.LoginStreak);
    }

    private static (long Gold, string? Item) GetRewardForDay(int day)
    {
        return day switch
        {
            1 => (10, null),
            2 => (20, null),
            3 => (50, null),
            5 => (0, "Mystery Wand"),
            7 => (200, null),
            14 => (0, "Exclusive Enchanted Robe"),
            30 => (500, "Legendary Phoenix Feather Wand"),
            _ => ((long)day * 10, null)
        };
    }
}
