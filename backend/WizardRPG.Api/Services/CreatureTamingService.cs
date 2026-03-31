using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.CreatureTaming;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface ICreatureTamingService
{
    Task<List<CreatureResponse>> GetAllCreaturesAsync();
    Task<List<PlayerCreatureResponse>> GetPlayerCreaturesAsync(Guid playerId);
    Task<ExploreResponse> ExploreAsync(Guid playerId);
    Task<PlayerCreatureResponse> TameCreatureAsync(Guid playerId, TameCreatureRequest request);
    Task<CareResponse> CareForCreatureAsync(Guid playerId, Guid creatureId, CareForCreatureRequest request);
    Task<Dictionary<string, int>> GetCreatureBonusesAsync(Guid playerId);
}

public class CreatureTamingService : ICreatureTamingService
{
    private readonly AppDbContext _db;

    public CreatureTamingService(AppDbContext db) => _db = db;

    public async Task<List<CreatureResponse>> GetAllCreaturesAsync()
    {
        var creatures = await _db.Creatures.OrderBy(c => c.Rarity).ThenBy(c => c.Name).ToListAsync();
        return creatures.Select(MapCreatureToResponse).ToList();
    }

    public async Task<List<PlayerCreatureResponse>> GetPlayerCreaturesAsync(Guid playerId)
    {
        var playerCreatures = await _db.PlayerCreatures
            .Include(pc => pc.Creature)
            .Where(pc => pc.PlayerId == playerId)
            .OrderByDescending(pc => pc.TamedAt)
            .ToListAsync();

        return playerCreatures.Select(MapPlayerCreatureToResponse).ToList();
    }

    public async Task<ExploreResponse> ExploreAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        const long exploreCost = 50;
        if (player.GoldCoins < exploreCost)
            throw new InvalidOperationException("Not enough gold to explore. You need 50 gold coins.");

        player.GoldCoins -= exploreCost;

        var roll = Random.Shared.Next(100);
        CreatureRarity? foundRarity = roll switch
        {
            < 5 => CreatureRarity.Legendary,
            < 20 => CreatureRarity.Rare,
            < 50 => CreatureRarity.Uncommon,
            _ => CreatureRarity.Common
        };

        var creatures = await _db.Creatures.Where(c => c.Rarity == foundRarity).ToListAsync();
        if (creatures.Count == 0)
        {
            await _db.SaveChangesAsync();
            return new ExploreResponse(false, null,
                "You ventured deep into the enchanted forest but found nothing this time. The magical creatures remain elusive...");
        }

        var found = creatures[Random.Shared.Next(creatures.Count)];
        await _db.SaveChangesAsync();

        var narrative = foundRarity switch
        {
            CreatureRarity.Legendary => $"Incredible! A mythical {found.Name} appears before you, radiating ancient power! This is a once-in-a-lifetime encounter!",
            CreatureRarity.Rare => $"Amazing! You spot a rare {found.Name} hiding among the magical flora. It watches you cautiously...",
            CreatureRarity.Uncommon => $"You discover a {found.Name} resting by a crystal stream. It seems curious about you.",
            _ => $"A wild {found.Name} emerges from the underbrush, playfully circling around you."
        };

        return new ExploreResponse(true, MapCreatureToResponse(found), narrative);
    }

    public async Task<PlayerCreatureResponse> TameCreatureAsync(Guid playerId, TameCreatureRequest request)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var creature = await _db.Creatures.FindAsync(request.CreatureId)
            ?? throw new KeyNotFoundException("Creature not found.");

        var playerCreature = new PlayerCreature
        {
            PlayerId = playerId,
            CreatureId = request.CreatureId,
            Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? creature.Name : request.Nickname,
            Happiness = 50,
            Loyalty = 0,
            Level = 1,
            TamedAt = DateTime.UtcNow
        };

        _db.PlayerCreatures.Add(playerCreature);
        await _db.SaveChangesAsync();

        playerCreature.Creature = creature;
        return MapPlayerCreatureToResponse(playerCreature);
    }

    public async Task<CareResponse> CareForCreatureAsync(Guid playerId, Guid creatureId, CareForCreatureRequest request)
    {
        var playerCreature = await _db.PlayerCreatures
            .Include(pc => pc.Creature)
            .FirstOrDefaultAsync(pc => pc.Id == creatureId && pc.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Creature not found in your collection.");

        var now = DateTime.UtcNow;
        var action = request.Action.ToLowerInvariant();

        int happinessChange = 0;
        int loyaltyChange = 0;
        int? levelUp = null;
        string narrative;

        switch (action)
        {
            case "feed":
                if (playerCreature.LastFedAt.HasValue &&
                    (now - playerCreature.LastFedAt.Value).TotalHours < 1)
                {
                    throw new InvalidOperationException(
                        "Your creature was fed recently. Please wait before feeding again.");
                }

                var player = await _db.Players.FindAsync(playerId)
                    ?? throw new KeyNotFoundException("Player not found.");

                const long feedCost = 20;
                if (player.GoldCoins < feedCost)
                    throw new InvalidOperationException("Not enough gold to feed your creature. You need 20 gold coins.");

                player.GoldCoins -= feedCost;
                happinessChange = 10;
                playerCreature.Happiness = Math.Min(100, playerCreature.Happiness + happinessChange);
                playerCreature.LastFedAt = now;
                narrative = $"You feed {playerCreature.Nickname} a delicious magical treat. It purrs with delight!";
                break;

            case "train":
                if (playerCreature.LastTrainedAt.HasValue &&
                    (now - playerCreature.LastTrainedAt.Value).TotalHours < 2)
                {
                    throw new InvalidOperationException(
                        "Your creature is still tired from the last training session. Please wait before training again.");
                }

                loyaltyChange = 5;
                playerCreature.Loyalty = Math.Min(100, playerCreature.Loyalty + loyaltyChange);
                playerCreature.LastTrainedAt = now;

                // Check level up thresholds: 25, 50, 75, 100
                int newLevel = playerCreature.Loyalty switch
                {
                    >= 100 => 5,
                    >= 75 => 4,
                    >= 50 => 3,
                    >= 25 => 2,
                    _ => 1
                };
                if (newLevel > playerCreature.Level)
                {
                    playerCreature.Level = newLevel;
                    levelUp = newLevel;
                }

                narrative = levelUp.HasValue
                    ? $"Excellent training session! {playerCreature.Nickname} has grown stronger and reached level {levelUp}!"
                    : $"You train with {playerCreature.Nickname}. Your bond grows stronger through the exercise.";
                break;

            case "rest":
                happinessChange = 15;
                loyaltyChange = playerCreature.Loyalty < 20 ? -5 : 0;
                playerCreature.Happiness = Math.Min(100, playerCreature.Happiness + happinessChange);
                playerCreature.Loyalty = Math.Max(0, playerCreature.Loyalty + loyaltyChange);
                narrative = loyaltyChange < 0
                    ? $"{playerCreature.Nickname} rests peacefully but seems a bit distant. Spend more time training together!"
                    : $"{playerCreature.Nickname} curls up for a restful nap. Its mood visibly brightens.";
                break;

            default:
                throw new ArgumentException("Invalid action. Use 'feed', 'train', or 'rest'.");
        }

        await _db.SaveChangesAsync();
        return new CareResponse(narrative, happinessChange, loyaltyChange, levelUp);
    }

    public async Task<Dictionary<string, int>> GetCreatureBonusesAsync(Guid playerId)
    {
        var loyalCreatures = await _db.PlayerCreatures
            .Include(pc => pc.Creature)
            .Where(pc => pc.PlayerId == playerId && pc.Loyalty > 50)
            .ToListAsync();

        var bonuses = new Dictionary<string, int>
        {
            ["gold"] = 0,
            ["magic"] = 0,
            ["strength"] = 0,
            ["wisdom"] = 0,
            ["speed"] = 0
        };

        foreach (var pc in loyalCreatures)
        {
            if (pc.Creature == null) continue;

            // Creatures with combined bonus types (e.g., "magic+wisdom")
            var bonusTypes = pc.Creature.BonusType.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var bonusType in bonusTypes)
            {
                var key = bonusType.ToLowerInvariant();
                if (bonuses.ContainsKey(key))
                {
                    bonuses[key] += pc.Creature.BonusValue * pc.Level;
                }
            }
        }

        return bonuses;
    }

    private static CreatureResponse MapCreatureToResponse(Creature c) => new(
        c.Id, c.Name, c.Description, c.Rarity,
        c.BaseHealth, c.BaseAttack, c.BonusType, c.BonusValue);

    private static PlayerCreatureResponse MapPlayerCreatureToResponse(PlayerCreature pc) => new(
        pc.Id, pc.CreatureId, pc.Creature?.Name ?? string.Empty, pc.Creature?.Description ?? string.Empty,
        pc.Creature?.Rarity ?? CreatureRarity.Common, pc.Nickname, pc.Happiness, pc.Loyalty,
        pc.Level, pc.Creature?.BonusType ?? string.Empty, pc.Creature?.BonusValue ?? 0,
        pc.LastFedAt, pc.LastTrainedAt, pc.TamedAt);
}
