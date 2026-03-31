using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.House;
using WizardRPG.Api.DTOs.Player;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IHouseService
{
    Task<List<HouseLeaderboardEntry>> GetHouseLeaderboardAsync();
    Task<List<HousePointsResponse>> GetHousePointsAsync(string house, int limit = 50);
    Task<HousePointsResponse> AwardHousePointsAsync(Guid playerId, int points, string activity);
    Task<PlayerProfileResponse> SelectHouseAsync(Guid playerId, string house);
}

public class HouseService : IHouseService
{
    private static readonly HashSet<string> ValidHouses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pyromancers", "Frostwardens", "Stormcallers", "Earthshapers"
    };

    private readonly AppDbContext _db;

    public HouseService(AppDbContext db) => _db = db;

    public async Task<List<HouseLeaderboardEntry>> GetHouseLeaderboardAsync()
    {
        var houseStats = await _db.HousePoints
            .GroupBy(hp => hp.House)
            .Select(g => new
            {
                House = g.Key,
                TotalPoints = (long)g.Sum(hp => hp.Points)
            })
            .ToListAsync();

        var memberCounts = await _db.Players
            .Where(p => !string.IsNullOrEmpty(p.House))
            .GroupBy(p => p.House)
            .Select(g => new { House = g.Key, Count = g.Count() })
            .ToListAsync();

        var memberCountDict = memberCounts.ToDictionary(m => m.House, m => m.Count, StringComparer.OrdinalIgnoreCase);

        return houseStats
            .Select(h => new HouseLeaderboardEntry(
                h.House,
                h.TotalPoints,
                memberCountDict.GetValueOrDefault(h.House, 0)))
            .OrderByDescending(h => h.TotalPoints)
            .ToList();
    }

    public async Task<List<HousePointsResponse>> GetHousePointsAsync(string house, int limit = 50)
    {
        var points = await _db.HousePoints
            .Include(hp => hp.Player)
            .Where(hp => hp.House == house)
            .OrderByDescending(hp => hp.EarnedAt)
            .Take(limit)
            .ToListAsync();

        return points.Select(hp => new HousePointsResponse(
            hp.Id, hp.PlayerId, hp.Player?.Username ?? string.Empty,
            hp.House, hp.Points, hp.Activity, hp.EarnedAt)).ToList();
    }

    public async Task<HousePointsResponse> AwardHousePointsAsync(Guid playerId, int points, string activity)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (string.IsNullOrWhiteSpace(player.House))
            throw new InvalidOperationException("Player has not joined a house.");

        var hp = new HousePoints
        {
            PlayerId = playerId,
            House = player.House,
            Points = points,
            Activity = activity
        };

        _db.HousePoints.Add(hp);
        await _db.SaveChangesAsync();

        return new HousePointsResponse(
            hp.Id, hp.PlayerId, player.Username,
            hp.House, hp.Points, hp.Activity, hp.EarnedAt);
    }

    public async Task<PlayerProfileResponse> SelectHouseAsync(Guid playerId, string house)
    {
        if (!ValidHouses.Contains(house))
            throw new ArgumentException($"Invalid house. Must be one of: {string.Join(", ", ValidHouses)}");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        player.House = house;
        await _db.SaveChangesAsync();

        var (tier, badge) = GetRankInfo(player.EloRating);
        return new PlayerProfileResponse(
            player.Id, player.Username, player.Email, player.GoldCoins,
            player.Level, player.Experience, player.MagicPower, player.Strength,
            player.Wisdom, player.Speed, player.ReferralCode, player.CreatedAt,
            player.IsAdmin, player.EloRating, player.House, tier, badge,
            player.HasCompletedOnboarding, player.LoginStreak);
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
