using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.DungeonCrawler;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IDungeonCrawlerService
{
    Task<(DungeonRunResponse Run, DungeonRoomResponse Room)> StartRunAsync(Guid playerId);
    Task<DungeonRoomResponse> GetCurrentRoomAsync(Guid runId);
    Task<DungeonActionResponse> MakeChoiceAsync(Guid playerId, Guid runId, DungeonActionRequest request);
    Task<DungeonRunResponse> EscapeAsync(Guid playerId, Guid runId);
    Task<List<DungeonRunResponse>> GetRunHistoryAsync(Guid playerId);
    Task<(DungeonRunResponse Run, DungeonRoomResponse Room)?> GetActiveRunAsync(Guid playerId);
}

public class DungeonCrawlerService : IDungeonCrawlerService
{
    private readonly AppDbContext _db;

    public DungeonCrawlerService(AppDbContext db) => _db = db;

    public async Task<(DungeonRunResponse Run, DungeonRoomResponse Room)> StartRunAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var existingRun = await _db.DungeonRuns
            .FirstOrDefaultAsync(r => r.PlayerId == playerId && r.Status == DungeonRunStatus.Active);
        if (existingRun != null)
            throw new InvalidOperationException("You already have an active dungeon run.");

        var maxHp = 80 + player.Strength / 2;
        var run = new DungeonRun
        {
            PlayerId = playerId,
            MaxHp = maxHp,
            CurrentHp = maxHp
        };

        _db.DungeonRuns.Add(run);
        await _db.SaveChangesAsync();

        var room = GenerateRoom(run.Id, run.CurrentFloor);
        return (MapRunToResponse(run), room);
    }

    public Task<DungeonRoomResponse> GetCurrentRoomAsync(Guid runId)
    {
        return Task.FromResult(GetCurrentRoomInternal(runId, 0));
    }

    public async Task<DungeonActionResponse> MakeChoiceAsync(Guid playerId, Guid runId, DungeonActionRequest request)
    {
        var run = await _db.DungeonRuns.FirstOrDefaultAsync(r => r.Id == runId && r.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Dungeon run not found.");

        if (run.Status != DungeonRunStatus.Active)
            throw new InvalidOperationException("This dungeon run is no longer active.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var room = GenerateRoom(run.Id, run.CurrentFloor);
        var choice = room.Choices.FirstOrDefault(c => c.Id == request.ChoiceId)
            ?? throw new ArgumentException("Invalid choice.");

        var rng = new Random(run.Id.GetHashCode() + run.CurrentFloor * 1000 + request.ChoiceId.GetHashCode());
        var (narrative, hpChange, goldChange, xpChange) = ResolveChoice(room.Type, choice.Id, run, player, rng);

        run.CurrentHp = Math.Clamp(run.CurrentHp + hpChange, 0, run.MaxHp);
        run.GoldCollected += goldChange;
        run.XpCollected += xpChange;

        bool runEnded = false;
        DungeonRoomResponse? nextRoom = null;

        if (run.CurrentHp <= 0)
        {
            run.Status = DungeonRunStatus.Defeated;
            run.EndedAt = DateTime.UtcNow;
            run.GoldCollected = 0;
            run.XpCollected = 0;
            runEnded = true;
            narrative += " You have been defeated! All collected loot is lost.";
        }
        else if (choice.Id != "go_back")
        {
            run.CurrentFloor++;
            nextRoom = GenerateRoom(run.Id, run.CurrentFloor);
        }
        else
        {
            nextRoom = GenerateRoom(run.Id, run.CurrentFloor);
        }

        await _db.SaveChangesAsync();

        return new DungeonActionResponse(
            narrative, hpChange, goldChange, xpChange,
            run.CurrentHp, run.GoldCollected, run.XpCollected,
            runEnded, nextRoom);
    }

    public async Task<DungeonRunResponse> EscapeAsync(Guid playerId, Guid runId)
    {
        var run = await _db.DungeonRuns.FirstOrDefaultAsync(r => r.Id == runId && r.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Dungeon run not found.");

        if (run.Status != DungeonRunStatus.Active)
            throw new InvalidOperationException("This dungeon run is no longer active.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        player.GoldCoins += run.GoldCollected;
        player.Experience += run.XpCollected;

        run.Status = DungeonRunStatus.Escaped;
        run.EndedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapRunToResponse(run);
    }

    public async Task<List<DungeonRunResponse>> GetRunHistoryAsync(Guid playerId)
    {
        var runs = await _db.DungeonRuns
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();

        return runs.Select(MapRunToResponse).ToList();
    }

    public async Task<(DungeonRunResponse Run, DungeonRoomResponse Room)?> GetActiveRunAsync(Guid playerId)
    {
        var run = await _db.DungeonRuns
            .FirstOrDefaultAsync(r => r.PlayerId == playerId && r.Status == DungeonRunStatus.Active);

        if (run == null) return null;

        var room = GenerateRoom(run.Id, run.CurrentFloor);
        return (MapRunToResponse(run), room);
    }

    private DungeonRoomResponse GetCurrentRoomInternal(Guid runId, int floor)
    {
        return GenerateRoom(runId, floor == 0 ? 1 : floor);
    }

    private static DungeonRoomResponse GenerateRoom(Guid runId, int floor)
    {
        var rng = new Random(runId.GetHashCode() + floor);
        var roomType = PickRoomType(rng, floor);
        var description = GenerateDescription(roomType, floor, rng);
        var choices = GenerateChoices(roomType);
        return new DungeonRoomResponse(roomType, description, choices);
    }

    private static RoomType PickRoomType(Random rng, int floor)
    {
        if (floor == 5 || floor == 10) return RoomType.Boss;

        var roll = rng.Next(100);

        if (floor <= 3)
        {
            return roll switch
            {
                < 40 => RoomType.Monster,
                < 70 => RoomType.Treasure,
                _ => RoomType.Rest
            };
        }

        if (floor <= 6)
        {
            return roll switch
            {
                < 30 => RoomType.Monster,
                < 50 => RoomType.Treasure,
                < 70 => RoomType.Trap,
                < 85 => RoomType.Merchant,
                _ => RoomType.Rest
            };
        }

        return roll switch
        {
            < 35 => RoomType.Monster,
            < 50 => RoomType.Treasure,
            < 70 => RoomType.Trap,
            < 85 => RoomType.Merchant,
            _ => RoomType.Rest
        };
    }

    private static string GenerateDescription(RoomType type, int floor, Random rng)
    {
        var descriptions = type switch
        {
            RoomType.Monster => new[]
            {
                "A snarling creature lurks in the shadows ahead.",
                "You hear growling echoing off the dungeon walls.",
                "A hostile beast blocks your path forward."
            },
            RoomType.Treasure => new[]
            {
                "A gleaming chest sits in the center of the room.",
                "Gold coins are scattered across the stone floor.",
                "You spot a hidden cache behind a crumbling wall."
            },
            RoomType.Trap => new[]
            {
                "The floor ahead looks suspiciously uneven.",
                "Strange runes glow faintly on the walls.",
                "You notice thin wires stretched across the corridor."
            },
            RoomType.Merchant => new[]
            {
                "A hooded figure beckons you from a dimly lit alcove.",
                "A traveling merchant has set up shop in this chamber.",
                "An old wizard offers their services for a price."
            },
            RoomType.Rest => new[]
            {
                "A quiet chamber with a small campfire provides respite.",
                "You find a safe alcove where you can catch your breath.",
                "A peaceful spring bubbles up from the dungeon floor."
            },
            RoomType.Boss => new[]
            {
                $"A massive guardian of floor {floor} awaits your challenge!",
                $"The dungeon boss of floor {floor} blocks the way forward!",
                $"An ancient protector stands guard over floor {floor}!"
            },
            _ => new[] { "You enter a mysterious room." }
        };

        return descriptions[rng.Next(descriptions.Length)];
    }

    private static List<DungeonChoiceResponse> GenerateChoices(RoomType type)
    {
        return type switch
        {
            RoomType.Monster =>
            [
                new("fight", "Fight the creature", "Medium"),
                new("sneak", "Sneak past", "Low"),
                new("cast_spell", "Cast a spell", "Medium")
            ],
            RoomType.Treasure =>
            [
                new("open_carefully", "Open carefully", "Low"),
                new("grab_and_run", "Grab and run", "Medium")
            ],
            RoomType.Trap =>
            [
                new("disarm", "Disarm the trap", "Medium"),
                new("jump_across", "Jump across", "Medium"),
                new("go_back", "Go back (safe)", "Low")
            ],
            RoomType.Merchant =>
            [
                new("buy_healing", "Buy healing (30 gold)", "Low"),
                new("browse_and_leave", "Browse and leave", "Low")
            ],
            RoomType.Rest =>
            [
                new("rest_here", "Rest here", "Low"),
                new("search_area", "Search the area", "Medium")
            ],
            RoomType.Boss =>
            [
                new("fight_boss", "Fight the boss", "High"),
                new("use_magic", "Use magic", "Medium")
            ],
            _ => [new("proceed", "Proceed", "Low")]
        };
    }

    private static (string Narrative, int HpChange, long GoldChange, int XpChange) ResolveChoice(
        RoomType roomType, string choiceId, DungeonRun run, Player player, Random rng)
    {
        var floor = run.CurrentFloor;
        return (roomType, choiceId) switch
        {
            (RoomType.Monster, "fight") => ResolveMonsterFight(floor, rng),
            (RoomType.Monster, "sneak") => ResolveSneakPast(floor, player, rng),
            (RoomType.Monster, "cast_spell") => ResolveCastSpell(floor, player, rng),
            (RoomType.Treasure, "open_carefully") => ResolveTreasureCareful(floor, rng),
            (RoomType.Treasure, "grab_and_run") => ResolveTreasureGrab(floor, rng),
            (RoomType.Trap, "disarm") => ResolveDisarmTrap(floor, player, rng),
            (RoomType.Trap, "jump_across") => ResolveJumpTrap(floor, player, rng),
            (RoomType.Trap, "go_back") => ("You carefully retreat to safety.", 0, 0, 0),
            (RoomType.Merchant, "buy_healing") => ResolveMerchantHeal(run),
            (RoomType.Merchant, "browse_and_leave") => ("You browse the wares but find nothing of interest.", 0, 0, 5),
            (RoomType.Rest, "rest_here") => ResolveRest(rng),
            (RoomType.Rest, "search_area") => ResolveSearchArea(floor, rng),
            (RoomType.Boss, "fight_boss") => ResolveBossFight(floor, rng),
            (RoomType.Boss, "use_magic") => ResolveBossMagic(floor, player, rng),
            _ => ("You proceed cautiously.", 0, 0, 5)
        };
    }

    private static (string, int, long, int) ResolveMonsterFight(int floor, Random rng)
    {
        var damage = rng.Next(10, 31) * (floor / 3 + 1);
        var gold = (long)(rng.Next(15, 41) * floor);
        var xp = rng.Next(20, 41) * floor;
        return ($"You fight bravely! You take {damage} damage but defeat the creature.", -damage, gold, xp);
    }

    private static (string, int, long, int) ResolveSneakPast(int floor, Player player, Random rng)
    {
        var successChance = 40 + player.Speed * 2;
        if (rng.Next(100) < successChance)
            return ("You slip past the creature unnoticed.", 0, 0, rng.Next(5, 15) * floor);

        var damage = rng.Next(10, 21) * (floor / 3 + 1);
        return ($"The creature spotted you! You take {damage} damage escaping.", -damage, 0, rng.Next(3, 8) * floor);
    }

    private static (string, int, long, int) ResolveCastSpell(int floor, Player player, Random rng)
    {
        var successChance = 30 + player.MagicPower * 2;
        if (rng.Next(100) < successChance)
        {
            var gold = (long)(rng.Next(20, 51) * floor);
            var xp = rng.Next(25, 51) * floor;
            return ($"Your spell obliterates the creature! You claim its treasure.", 0, gold, xp);
        }

        var damage = rng.Next(15, 31) * (floor / 3 + 1);
        var partialGold = (long)(rng.Next(5, 16) * floor);
        return ($"Your spell fizzles! The creature strikes back for {damage} damage.", -damage, partialGold, rng.Next(10, 20) * floor);
    }

    private static (string, int, long, int) ResolveTreasureCareful(int floor, Random rng)
    {
        var gold = (long)(rng.Next(20, 41) * floor);
        return ($"You carefully open the chest and find {gold} gold!", 0, gold, rng.Next(5, 15));
    }

    private static (string, int, long, int) ResolveTreasureGrab(int floor, Random rng)
    {
        if (rng.Next(100) < 60)
        {
            var gold = (long)(rng.Next(40, 61) * floor);
            return ($"You grab a massive haul of {gold} gold!", 0, gold, rng.Next(10, 20));
        }

        var damage = rng.Next(10, 21);
        var partialGold = (long)(rng.Next(20, 36) * floor);
        return ($"A trap springs! You take {damage} damage but still grab {partialGold} gold.", -damage, partialGold, rng.Next(8, 15));
    }

    private static (string, int, long, int) ResolveDisarmTrap(int floor, Player player, Random rng)
    {
        var successChance = 30 + player.Wisdom * 2;
        if (rng.Next(100) < successChance)
        {
            var gold = (long)(rng.Next(10, 26) * floor);
            return ($"You skillfully disarm the trap and salvage {gold} gold in parts!", 0, gold, rng.Next(15, 30) * floor);
        }

        var damage = rng.Next(10, 26) * (floor / 2 + 1);
        return ($"The trap triggers! You take {damage} damage.", -damage, 0, rng.Next(5, 10) * floor);
    }

    private static (string, int, long, int) ResolveJumpTrap(int floor, Player player, Random rng)
    {
        var successChance = 35 + player.Speed * 2;
        if (rng.Next(100) < successChance)
            return ("You leap across the trap with agility!", 0, 0, rng.Next(10, 20) * floor);

        var damage = rng.Next(10, 26) * (floor / 2 + 1);
        return ($"You stumble into the trap! You take {damage} damage.", -damage, 0, rng.Next(3, 8) * floor);
    }

    private static (string, int, long, int) ResolveMerchantHeal(DungeonRun run)
    {
        if (run.GoldCollected < 30)
            return ("You don't have enough gold for healing. The merchant waves you away.", 0, 0, 0);

        var healAmount = Math.Min(30, run.MaxHp - run.CurrentHp);
        return ($"The merchant heals you for {healAmount} HP.", healAmount, -30, 5);
    }

    private static (string, int, long, int) ResolveRest(Random rng)
    {
        var heal = rng.Next(15, 31);
        return ($"You rest and recover {heal} HP.", heal, 0, 5);
    }

    private static (string, int, long, int) ResolveSearchArea(int floor, Random rng)
    {
        if (rng.Next(100) < 50)
        {
            var gold = (long)(rng.Next(10, 31) * floor);
            return ($"You find a hidden stash of {gold} gold!", 0, gold, rng.Next(5, 15));
        }

        var damage = rng.Next(5, 16);
        return ($"A hidden trap springs! You take {damage} damage.", -damage, 0, rng.Next(3, 8));
    }

    private static (string, int, long, int) ResolveBossFight(int floor, Random rng)
    {
        var damage = rng.Next(20, 51) * (floor / 3 + 1);
        var gold = (long)(rng.Next(50, 101) * floor);
        var xp = rng.Next(50, 101) * floor;
        return ($"An epic battle! The boss deals {damage} damage but you prevail!", -damage, gold, xp);
    }

    private static (string, int, long, int) ResolveBossMagic(int floor, Player player, Random rng)
    {
        var successChance = 25 + player.MagicPower * 2;
        if (rng.Next(100) < successChance)
        {
            var gold = (long)(rng.Next(60, 121) * floor);
            var xp = rng.Next(60, 121) * floor;
            return ($"Your powerful magic overwhelms the boss!", 0, gold, xp);
        }

        var damage = rng.Next(25, 51) * (floor / 3 + 1);
        var partialGold = (long)(rng.Next(20, 51) * floor);
        return ($"The boss resists your magic and strikes for {damage} damage! You defeat it eventually.", -damage, partialGold, rng.Next(30, 60) * floor);
    }

    private static DungeonRunResponse MapRunToResponse(DungeonRun r) => new(
        r.Id, r.CurrentFloor, r.CurrentHp, r.MaxHp,
        r.GoldCollected, r.XpCollected, r.Status,
        r.StartedAt, r.EndedAt);
}
