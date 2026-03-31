using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Player;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IPlayerService
{
    Task<PlayerProfileResponse> GetProfileAsync(Guid playerId);
    Task<PlayerProfileResponse> UpdateProfileAsync(Guid playerId, UpdateProfileRequest request);
    Task<PlayerProfileResponse> AddExperienceAsync(Guid playerId, long amount);
    Task<List<PlayerProfileResponse>> GetLeaderboardAsync(int top = 10);
    Task<BattleStatsResponse> GetBattleStatsAsync(Guid playerId);
}

public class PlayerService : IPlayerService
{
    private readonly AppDbContext _db;

    public PlayerService(AppDbContext db) => _db = db;

    public async Task<PlayerProfileResponse> GetProfileAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");
        return MapToResponse(player);
    }

    public async Task<PlayerProfileResponse> UpdateProfileAsync(Guid playerId, UpdateProfileRequest request)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (await _db.Players.AnyAsync(p => p.Username == request.Username && p.Id != playerId))
                throw new InvalidOperationException("Username already taken.");
            player.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _db.Players.AnyAsync(p => p.Email == request.Email && p.Id != playerId))
                throw new InvalidOperationException("Email already in use.");
            player.Email = request.Email;
        }

        await _db.SaveChangesAsync();
        return MapToResponse(player);
    }

    public async Task<PlayerProfileResponse> AddExperienceAsync(Guid playerId, long amount)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        player.Experience += amount;

        // Level up formula: each level requires level * 1000 XP
        while (player.Experience >= (long)player.Level * 1000)
        {
            player.Experience -= (long)player.Level * 1000;
            player.Level++;
            // Stat boost on level up
            player.MagicPower += 2;
            player.Strength += 2;
            player.Wisdom += 2;
            player.Speed += 1;
        }

        await _db.SaveChangesAsync();
        return MapToResponse(player);
    }

    public async Task<List<PlayerProfileResponse>> GetLeaderboardAsync(int top = 10)
    {
        var players = await _db.Players
            .OrderByDescending(p => p.Level)
            .ThenByDescending(p => p.Experience)
            .Take(top)
            .ToListAsync();
        return players.Select(MapToResponse).ToList();
    }

    public async Task<BattleStatsResponse> GetBattleStatsAsync(Guid playerId)
    {
        _ = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var battles = await _db.Battles
            .Where(b => (b.ChallengerId == playerId || b.DefenderId == playerId)
                        && b.Status == BattleStatus.Finished)
            .OrderByDescending(b => b.FinishedAt)
            .ToListAsync();

        var wins = battles.Count(b => b.WinnerId == playerId);
        var losses = battles.Count - wins;
        var winRate = battles.Count > 0 ? (double)wins / battles.Count : 0.0;

        var turns = await _db.BattleTurns
            .Include(t => t.Spell)
            .Where(t => t.Battle!.ChallengerId == playerId || t.Battle!.DefenderId == playerId)
            .Where(t => t.Battle!.Status == BattleStatus.Finished)
            .ToListAsync();

        long totalDamageDealt = turns.Where(t => t.AttackerId == playerId).Sum(t => (long)t.DamageDealt);
        long totalDamageReceived = turns.Where(t => t.AttackerId != playerId).Sum(t => (long)t.DamageDealt);

        var mostUsedSpell = turns
            .Where(t => t.AttackerId == playerId && t.Spell != null)
            .GroupBy(t => t.Spell!.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // Calculate win streaks
        int currentStreak = 0;
        int bestStreak = 0;
        int tempStreak = 0;
        foreach (var b in battles)
        {
            if (b.WinnerId == playerId)
            {
                tempStreak++;
                if (tempStreak > bestStreak)
                    bestStreak = tempStreak;
            }
            else
            {
                tempStreak = 0;
            }
        }
        // Current streak from most recent
        foreach (var b in battles)
        {
            if (b.WinnerId == playerId)
                currentStreak++;
            else
                break;
        }

        return new BattleStatsResponse(
            battles.Count, wins, losses, Math.Round(winRate, 4),
            totalDamageDealt, totalDamageReceived,
            mostUsedSpell, currentStreak, bestStreak);
    }

    private static PlayerProfileResponse MapToResponse(Player p)
    {
        var (tier, badge) = GetRankInfo(p.EloRating);
        return new(
            p.Id, p.Username, p.Email, p.GoldCoins, p.Level,
            p.Experience, p.MagicPower, p.Strength, p.Wisdom,
            p.Speed, p.ReferralCode, p.CreatedAt, p.IsAdmin,
            p.EloRating, p.House, tier, badge,
            p.HasCompletedOnboarding, p.LoginStreak);
    }

    private static (string Tier, string Badge) GetRankInfo(int elo) => elo switch
    {
        >= 2000 => ("Archmage", "🔮"),
        >= 1600 => ("Master Wizard", "⭐"),
        >= 1200 => ("Journeyman", "🌟"),
        >= 800 => ("Apprentice", "📖"),
        _ => ("Novice", "🪄")
    };
}
