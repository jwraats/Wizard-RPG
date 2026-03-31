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

    private static PlayerProfileResponse MapToResponse(Player p) => new(
        p.Id, p.Username, p.Email, p.GoldCoins, p.Level,
        p.Experience, p.MagicPower, p.Strength, p.Wisdom,
        p.Speed, p.ReferralCode, p.CreatedAt, p.IsAdmin);
}
