using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.Battle;

public record BattleResponse(
    Guid Id,
    Guid ChallengerId,
    string ChallengerUsername,
    Guid DefenderId,
    string DefenderUsername,
    BattleStatus Status,
    Guid? WinnerId,
    string? WinnerUsername,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string? NarratorStory,
    List<BattleTurnResponse> Turns);

public record BattleTurnResponse(
    int TurnNumber,
    Guid AttackerId,
    string AttackerUsername,
    string SpellName,
    SpellElement Element,
    int DamageDealt,
    string? Narrative);

public record ChallengeBattleRequest(Guid DefenderId);

public record CastSpellRequest(Guid SpellId);

public record SpellResponse(
    Guid Id,
    string Name,
    string Description,
    int ManaCost,
    int BaseDamage,
    string Effect,
    SpellElement Element);
