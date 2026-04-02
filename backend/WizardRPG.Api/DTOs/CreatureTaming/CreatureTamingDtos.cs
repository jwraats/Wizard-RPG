using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.CreatureTaming;

public record CreatureResponse(
    Guid Id, string Name, string Description, CreatureRarity Rarity,
    int BaseHealth, int BaseAttack, string BonusType, int BonusValue);

public record PlayerCreatureResponse(
    Guid Id, Guid CreatureId, string CreatureName, string Description,
    CreatureRarity Rarity, string Nickname, int Happiness, int Loyalty,
    int Level, string BonusType, int BonusValue,
    DateTime? LastFedAt, DateTime? LastTrainedAt, DateTime TamedAt);

public record ExploreResponse(bool Found, CreatureResponse? Creature, string Narrative);

public record TameCreatureRequest(Guid CreatureId, string? Nickname);

public record CareForCreatureRequest(string Action); // "feed", "train", "rest"

public record CareResponse(string Narrative, int HappinessChange, int LoyaltyChange, int? LevelUp);
