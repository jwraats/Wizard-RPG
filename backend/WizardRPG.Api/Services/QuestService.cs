using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Quest;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IQuestService
{
    Task<List<QuestResponse>> GetPlayerQuestsAsync(Guid playerId);
    Task GenerateDailyQuestsAsync(Guid playerId);
    Task GenerateWeeklyQuestsAsync(Guid playerId);
    Task UpdateQuestProgressAsync(Guid playerId, string activityType, int count = 1);
}

public class QuestService : IQuestService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public QuestService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<List<QuestResponse>> GetPlayerQuestsAsync(Guid playerId)
    {
        // Expire stale quests
        var now = DateTime.UtcNow;
        var expired = await _db.Quests
            .Where(q => q.PlayerId == playerId && q.Status == QuestStatus.Active && q.ExpiresAt <= now)
            .ToListAsync();

        foreach (var q in expired)
            q.Status = QuestStatus.Expired;

        if (expired.Count > 0)
            await _db.SaveChangesAsync();

        var quests = await _db.Quests
            .Where(q => q.PlayerId == playerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        return quests.Select(q => new QuestResponse(
            q.Id, q.Title, q.Description, q.Type, q.Status,
            q.TargetCount, q.CurrentCount, q.GoldReward, q.XpReward, q.ExpiresAt)).ToList();
    }

    public async Task GenerateDailyQuestsAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        var hasActive = await _db.Quests.AnyAsync(q =>
            q.PlayerId == playerId && q.Type == QuestType.Daily && q.Status == QuestStatus.Active && q.ExpiresAt > now);

        if (hasActive) return;

        var dailyQuests = new List<Quest>
        {
            new()
            {
                PlayerId = playerId,
                Title = "Win 2 battles",
                Description = "Win 2 PvP battles today",
                Type = QuestType.Daily,
                TargetCount = 2,
                GoldReward = 50,
                XpReward = 25,
                ExpiresAt = now.AddHours(24)
            },
            new()
            {
                PlayerId = playerId,
                Title = "Place a broom race bet",
                Description = "Place a bet on any broom race",
                Type = QuestType.Daily,
                TargetCount = 1,
                GoldReward = 20,
                XpReward = 10,
                ExpiresAt = now.AddHours(24)
            },
            new()
            {
                PlayerId = playerId,
                Title = "Deposit 100 gold in the bank",
                Description = "Deposit at least 100 gold into your bank account",
                Type = QuestType.Daily,
                TargetCount = 1,
                GoldReward = 30,
                XpReward = 15,
                ExpiresAt = now.AddHours(24)
            }
        };

        _db.Quests.AddRange(dailyQuests);
        await _db.SaveChangesAsync();
    }

    public async Task GenerateWeeklyQuestsAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        var hasActive = await _db.Quests.AnyAsync(q =>
            q.PlayerId == playerId && q.Type == QuestType.Weekly && q.Status == QuestStatus.Active && q.ExpiresAt > now);

        if (hasActive) return;

        var weeklyQuests = new List<Quest>
        {
            new()
            {
                PlayerId = playerId,
                Title = "Win 10 battles",
                Description = "Win 10 PvP battles this week",
                Type = QuestType.Weekly,
                TargetCount = 10,
                GoldReward = 300,
                XpReward = 150,
                ExpiresAt = now.AddDays(7)
            },
            new()
            {
                PlayerId = playerId,
                Title = "Earn 500 gold",
                Description = "Accumulate 500 gold from any source this week",
                Type = QuestType.Weekly,
                TargetCount = 500,
                GoldReward = 200,
                XpReward = 100,
                ExpiresAt = now.AddDays(7)
            }
        };

        _db.Quests.AddRange(weeklyQuests);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuestProgressAsync(Guid playerId, string activityType, int count = 1)
    {
        var now = DateTime.UtcNow;
        var activeQuests = await _db.Quests
            .Where(q => q.PlayerId == playerId && q.Status == QuestStatus.Active && q.ExpiresAt > now)
            .ToListAsync();

        foreach (var quest in activeQuests)
        {
            bool matches = activityType switch
            {
                "battle_win" => quest.Title.Contains("battle", StringComparison.OrdinalIgnoreCase),
                "broom_bet" => quest.Title.Contains("broom", StringComparison.OrdinalIgnoreCase),
                "bank_deposit" => quest.Title.Contains("Deposit", StringComparison.OrdinalIgnoreCase),
                "gold_earned" => quest.Title.Contains("Earn", StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            if (!matches) continue;

            quest.CurrentCount += count;

            if (quest.CurrentCount >= quest.TargetCount)
            {
                quest.Status = QuestStatus.Completed;

                var player = await _db.Players.FindAsync(playerId);
                if (player != null)
                {
                    player.GoldCoins += quest.GoldReward;
                    player.Experience += quest.XpReward;
                }

                await _notifications.CreateNotificationAsync(playerId,
                    "Quest Completed!",
                    $"You completed '{quest.Title}' and earned {quest.GoldReward} gold and {quest.XpReward} XP!",
                    "quest_complete");
            }
        }

        await _db.SaveChangesAsync();
    }
}
