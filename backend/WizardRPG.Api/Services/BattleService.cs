using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Battle;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IBattleService
{
    Task<List<SpellResponse>> GetSpellsAsync();
    Task<BattleResponse> ChallengeBattleAsync(Guid challengerId, Guid defenderId);
    Task<BattleResponse> GetBattleAsync(Guid battleId);
    Task<List<BattleResponse>> GetPlayerBattlesAsync(Guid playerId);
    Task<BattleResponse> ExecuteTurnAsync(Guid battleId, Guid attackerId, Guid spellId);
    Task<BattleResponse> AcceptBattleAsync(Guid battleId, Guid defenderId);
}

public class BattleService : IBattleService
{
    private readonly AppDbContext _db;
    private readonly ILLMNarratorService _narrator;
    private readonly IEquipmentService _equipmentService;
    private readonly INotificationService _notificationService;
    private readonly IQuestService _questService;
    private readonly IAchievementService _achievementService;

    public BattleService(
        AppDbContext db,
        ILLMNarratorService narrator,
        IEquipmentService equipmentService,
        INotificationService notificationService,
        IQuestService questService,
        IAchievementService achievementService)
    {
        _db = db;
        _narrator = narrator;
        _equipmentService = equipmentService;
        _notificationService = notificationService;
        _questService = questService;
        _achievementService = achievementService;
    }

    public async Task<List<SpellResponse>> GetSpellsAsync()
    {
        var spells = await _db.Spells.ToListAsync();
        return spells.Select(MapSpellToResponse).ToList();
    }

    public async Task<BattleResponse> ChallengeBattleAsync(Guid challengerId, Guid defenderId)
    {
        if (challengerId == defenderId)
            throw new InvalidOperationException("You cannot challenge yourself.");

        var defender = await _db.Players.FindAsync(defenderId)
            ?? throw new KeyNotFoundException("Defender not found.");

        var battle = new Battle
        {
            ChallengerId = challengerId,
            DefenderId = defenderId,
            Status = BattleStatus.Pending
        };

        _db.Battles.Add(battle);
        await _db.SaveChangesAsync();
        return await GetBattleAsync(battle.Id);
    }

    public async Task<BattleResponse> AcceptBattleAsync(Guid battleId, Guid defenderId)
    {
        var battle = await _db.Battles.FindAsync(battleId)
            ?? throw new KeyNotFoundException("Battle not found.");

        if (battle.DefenderId != defenderId)
            throw new UnauthorizedAccessException("Only the defender can accept this battle.");

        if (battle.Status != BattleStatus.Pending)
            throw new InvalidOperationException("Battle is not in pending state.");

        battle.Status = BattleStatus.Active;
        battle.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetBattleAsync(battleId);
    }

    public async Task<BattleResponse> GetBattleAsync(Guid battleId)
    {
        var battle = await LoadBattleAsync(battleId)
            ?? throw new KeyNotFoundException("Battle not found.");
        return MapToResponse(battle);
    }

    public async Task<List<BattleResponse>> GetPlayerBattlesAsync(Guid playerId)
    {
        var battles = await _db.Battles
            .Include(b => b.Challenger)
            .Include(b => b.Defender)
            .Include(b => b.Winner)
            .Include(b => b.Turns).ThenInclude(t => t.Attacker)
            .Include(b => b.Turns).ThenInclude(t => t.Spell)
            .Where(b => b.ChallengerId == playerId || b.DefenderId == playerId)
            .OrderByDescending(b => b.StartedAt)
            .ToListAsync();

        return battles.Select(MapToResponse).ToList();
    }

    public async Task<BattleResponse> ExecuteTurnAsync(Guid battleId, Guid attackerId, Guid spellId)
    {
        var battle = await LoadBattleAsync(battleId)
            ?? throw new KeyNotFoundException("Battle not found.");

        if (battle.Status != BattleStatus.Active)
            throw new InvalidOperationException("Battle is not active.");

        if (battle.ChallengerId != attackerId && battle.DefenderId != attackerId)
            throw new UnauthorizedAccessException("You are not part of this battle.");

        var turnNumber = battle.Turns.Count + 1;
        var lastAttackerId = battle.Turns.LastOrDefault()?.AttackerId;

        // Alternate turns - challenger goes first
        var expectedAttacker = turnNumber == 1 || lastAttackerId == battle.DefenderId
            ? battle.ChallengerId
            : battle.DefenderId;

        if (attackerId != expectedAttacker)
            throw new InvalidOperationException("It is not your turn.");

        var attacker = await _db.Players.FindAsync(attackerId)
            ?? throw new KeyNotFoundException("Attacker not found.");

        var defenderId = attackerId == battle.ChallengerId ? battle.DefenderId : battle.ChallengerId;
        var defender = await _db.Players.FindAsync(defenderId)
            ?? throw new KeyNotFoundException("Defender not found.");

        var spell = await _db.Spells.FindAsync(spellId)
            ?? throw new KeyNotFoundException("Spell not found.");

        var damage = await CalculateDamageWithEquipmentAsync(attacker, spell);
        var narrative = await _narrator.GenerateTurnNarrativeAsync(attacker.Username, defender.Username, spell.Name, damage);

        var turn = new BattleTurn
        {
            BattleId = battleId,
            AttackerId = attackerId,
            SpellId = spellId,
            DamageDealt = damage,
            TurnNumber = turnNumber,
            Narrative = narrative
        };

        _db.BattleTurns.Add(turn);

        // Check if battle is over (after 10 turns, or if total damage threshold reached)
        var challenger = battle.ChallengerId == attackerId ? attacker : defender;
        var defenderPlayer = battle.DefenderId == attackerId ? attacker : defender;

        var challengerDamage = battle.Turns
            .Where(t => t.AttackerId == battle.ChallengerId)
            .Sum(t => t.DamageDealt) + (attackerId == battle.ChallengerId ? damage : 0);

        var defenderDamage = battle.Turns
            .Where(t => t.AttackerId == battle.DefenderId)
            .Sum(t => t.DamageDealt) + (attackerId == battle.DefenderId ? damage : 0);

        if (turnNumber >= 10)
        {
            battle.Status = BattleStatus.Finished;
            battle.FinishedAt = DateTime.UtcNow;
            battle.WinnerId = challengerDamage >= defenderDamage
                ? battle.ChallengerId
                : battle.DefenderId;

            var loserId = battle.WinnerId == battle.ChallengerId
                ? battle.DefenderId
                : battle.ChallengerId;

            // Reward experience and gold
            var winner = await _db.Players.FindAsync(battle.WinnerId);
            var loser = await _db.Players.FindAsync(loserId);
            if (winner != null)
            {
                winner.Experience += 100;
                winner.GoldCoins += 50;
            }

            // ELO rating changes
            if (winner != null && loser != null)
            {
                var (winnerChange, loserChange) = CalculateEloChange(winner.EloRating, loser.EloRating);
                winner.EloRating = Math.Max(0, winner.EloRating + winnerChange);
                loser.EloRating = Math.Max(0, loser.EloRating + loserChange);
            }

            // Award house points for PvP win
            if (winner != null && !string.IsNullOrWhiteSpace(winner.House))
            {
                _db.HousePoints.Add(new HousePoints
                {
                    PlayerId = winner.Id,
                    House = winner.House,
                    Points = 10,
                    Activity = "PvP Win"
                });
            }

            // Update quest progress
            if (winner != null)
                await _questService.UpdateQuestProgressAsync(winner.Id, "battle_win");

            // Check achievements for both players
            if (winner != null)
                await _achievementService.CheckAndAwardAchievementsAsync(winner.Id);
            if (loser != null)
                await _achievementService.CheckAndAwardAchievementsAsync(loser.Id);

            // Notifications
            if (winner != null)
                await _notificationService.CreateNotificationAsync(winner.Id,
                    "Battle Won!", "You won a PvP battle and earned 50 gold and 100 XP!", "battle_result");
            if (loser != null)
                await _notificationService.CreateNotificationAsync(loser.Id,
                    "Battle Lost", "You lost a PvP battle. Better luck next time!", "battle_result");

            battle.NarratorStory = await _narrator.GenerateBattleStoryAsync(
                battle.Turns.Select(t => t.Narrative ?? string.Empty).ToList());
        }

        await _db.SaveChangesAsync();
        return await GetBattleAsync(battleId);
    }

    private static int CalculateDamage(Player attacker, Spell spell)
    {
        return CalculateDamageWithBonuses(attacker, spell, 0, 0, 0, 0);
    }

    private async Task<int> CalculateDamageWithEquipmentAsync(Player attacker, Spell spell)
    {
        var (magicBonus, strengthBonus, wisdomBonus, speedBonus) =
            await _equipmentService.GetEquipmentBonusesAsync(attacker.Id);
        return CalculateDamageWithBonuses(attacker, spell, magicBonus, strengthBonus, wisdomBonus, speedBonus);
    }

    private static int CalculateDamageWithBonuses(Player attacker, Spell spell,
        int magicBonus, int strengthBonus, int wisdomBonus, int speedBonus)
    {
        var statBonus = spell.Element switch
        {
            SpellElement.Fire or SpellElement.Arcane => attacker.MagicPower + magicBonus,
            SpellElement.Ice => attacker.Wisdom + wisdomBonus,
            SpellElement.Lightning => attacker.Speed + speedBonus,
            SpellElement.Earth => attacker.Strength + strengthBonus,
            _ => attacker.MagicPower + magicBonus
        };

        var damage = spell.BaseDamage + (statBonus / 5);
        var variance = Random.Shared.Next(-5, 6);
        return Math.Max(1, damage + variance);
    }

    private static (int winnerChange, int loserChange) CalculateEloChange(int winnerRating, int loserRating)
    {
        const int k = 32;
        double expectedWinner = 1.0 / (1.0 + Math.Pow(10.0, (loserRating - winnerRating) / 400.0));
        double expectedLoser = 1.0 / (1.0 + Math.Pow(10.0, (winnerRating - loserRating) / 400.0));
        int winnerChange = (int)Math.Round(k * (1.0 - expectedWinner));
        int loserChange = (int)Math.Round(k * (0.0 - expectedLoser));
        return (winnerChange, loserChange);
    }

    private async Task<Battle?> LoadBattleAsync(Guid battleId) =>
        await _db.Battles
            .Include(b => b.Challenger)
            .Include(b => b.Defender)
            .Include(b => b.Winner)
            .Include(b => b.Turns).ThenInclude(t => t.Attacker)
            .Include(b => b.Turns).ThenInclude(t => t.Spell)
            .FirstOrDefaultAsync(b => b.Id == battleId);

    private static BattleResponse MapToResponse(Battle b) => new(
        b.Id,
        b.ChallengerId,
        b.Challenger?.Username ?? string.Empty,
        b.DefenderId,
        b.Defender?.Username ?? string.Empty,
        b.Status,
        b.WinnerId,
        b.Winner?.Username,
        b.StartedAt,
        b.FinishedAt,
        b.NarratorStory,
        b.Turns.OrderBy(t => t.TurnNumber).Select(t => new BattleTurnResponse(
            t.TurnNumber,
            t.AttackerId,
            t.Attacker?.Username ?? string.Empty,
            t.Spell?.Name ?? string.Empty,
            t.Spell?.Element ?? SpellElement.Arcane,
            t.DamageDealt,
            t.Narrative)).ToList());

    private static SpellResponse MapSpellToResponse(Spell s) => new(
        s.Id, s.Name, s.Description, s.ManaCost, s.BaseDamage, s.Effect, s.Element);
}
