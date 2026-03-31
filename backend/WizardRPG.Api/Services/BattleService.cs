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

    public BattleService(AppDbContext db, ILLMNarratorService narrator)
    {
        _db = db;
        _narrator = narrator;
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

        var damage = CalculateDamage(attacker, spell);
        var narrative = _narrator.GenerateTurnNarrative(attacker.Username, defender.Username, spell.Name, damage);

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

            // Reward experience
            var winner = await _db.Players.FindAsync(battle.WinnerId);
            if (winner != null)
            {
                winner.Experience += 100;
                winner.GoldCoins += 50;
            }

            battle.NarratorStory = _narrator.GenerateBattleStory(
                battle.Turns.Select(t => t.Narrative ?? string.Empty).ToList());
        }

        await _db.SaveChangesAsync();
        return await GetBattleAsync(battleId);
    }

    private static int CalculateDamage(Player attacker, Spell spell)
    {
        var statBonus = spell.Element switch
        {
            SpellElement.Fire or SpellElement.Arcane => attacker.MagicPower,
            SpellElement.Ice => attacker.Wisdom,
            SpellElement.Lightning => attacker.Speed,
            SpellElement.Earth => attacker.Strength,
            _ => attacker.MagicPower
        };

        var damage = spell.BaseDamage + (statBonus / 5);
        var variance = new Random().Next(-5, 6);
        return Math.Max(1, damage + variance);
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
