using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Achievement;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IAchievementService
{
    Task<List<AchievementResponse>> GetPlayerAchievementsAsync(Guid playerId);
    Task CheckAndAwardAchievementsAsync(Guid playerId);
}

public class AchievementService : IAchievementService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public AchievementService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<List<AchievementResponse>> GetPlayerAchievementsAsync(Guid playerId)
    {
        var achievements = await _db.Achievements
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.UnlockedAt)
            .ToListAsync();

        return achievements.Select(a => new AchievementResponse(
            a.Id, a.Key, a.Name, a.Description, a.UnlockedAt)).ToList();
    }

    public async Task CheckAndAwardAchievementsAsync(Guid playerId)
    {
        var existing = await _db.Achievements
            .Where(a => a.PlayerId == playerId)
            .Select(a => a.Key)
            .ToListAsync();

        var existingKeys = new HashSet<string>(existing);

        // First Blood - Win your first battle
        if (!existingKeys.Contains("first_blood"))
        {
            var hasWin = await _db.Battles.AnyAsync(b =>
                b.WinnerId == playerId && b.Status == BattleStatus.Finished);
            if (hasWin)
                await AwardAchievementAsync(playerId, "first_blood", "First Blood", "Win your first battle.");
        }

        // Spell Master - Use every spell at least once
        if (!existingKeys.Contains("spell_master"))
        {
            var totalSpells = await _db.Spells.CountAsync();
            if (totalSpells > 0)
            {
                var usedSpells = await _db.BattleTurns
                    .Where(t => t.AttackerId == playerId)
                    .Select(t => t.SpellId)
                    .Distinct()
                    .CountAsync();
                if (usedSpells >= totalSpells)
                    await AwardAchievementAsync(playerId, "spell_master", "Spell Master", "Use every spell at least once.");
            }
        }

        // Gold Hoarder - Accumulate 10,000 gold
        if (!existingKeys.Contains("gold_hoarder"))
        {
            var player = await _db.Players.FindAsync(playerId);
            var bankBalance = await _db.BankAccounts
                .Where(b => b.PlayerId == playerId)
                .Select(b => b.GoldBalance)
                .FirstOrDefaultAsync();

            if (player != null && (player.GoldCoins + bankBalance) >= 10000)
                await AwardAchievementAsync(playerId, "gold_hoarder", "Gold Hoarder", "Accumulate 10,000 gold.");
        }

        // Social Butterfly - Join a fellowship
        if (!existingKeys.Contains("social_butterfly"))
        {
            var inFellowship = await _db.FellowshipMembers.AnyAsync(fm => fm.PlayerId == playerId);
            if (inFellowship)
                await AwardAchievementAsync(playerId, "social_butterfly", "Social Butterfly", "Join a fellowship.");
        }

        // Veteran Duelist - Win 50 battles
        if (!existingKeys.Contains("veteran"))
        {
            var wins = await _db.Battles.CountAsync(b =>
                b.WinnerId == playerId && b.Status == BattleStatus.Finished);
            if (wins >= 50)
                await AwardAchievementAsync(playerId, "veteran", "Veteran Duelist", "Win 50 battles.");
        }

        // Grand Wizard - Reach Grand Wizard rank (ELO 1600+)
        if (!existingKeys.Contains("grand_wizard"))
        {
            var player = await _db.Players.FindAsync(playerId);
            if (player != null && player.EloRating >= 1600)
                await AwardAchievementAsync(playerId, "grand_wizard", "Grand Wizard", "Reach Grand Wizard rank (ELO 1600+).");
        }

        await _db.SaveChangesAsync();
    }

    private async Task AwardAchievementAsync(Guid playerId, string key, string name, string description)
    {
        _db.Achievements.Add(new Achievement
        {
            PlayerId = playerId,
            Key = key,
            Name = name,
            Description = description
        });

        await _notifications.CreateNotificationAsync(playerId,
            "Achievement Unlocked!",
            $"You earned the '{name}' achievement: {description}",
            "achievement");
    }
}
