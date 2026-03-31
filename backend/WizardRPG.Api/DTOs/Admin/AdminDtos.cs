using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.Admin;

public record AdminPlayerResponse(
    Guid Id,
    string Username,
    string Email,
    long GoldCoins,
    int Level,
    long Experience,
    bool IsAdmin,
    DateTime CreatedAt);

public record GrantItemRequest(Guid PlayerId, Guid ItemId);
public record RevokeItemRequest(Guid BankItemId);
public record SetGoldRequest(Guid PlayerId, long Amount);
public record SetAdminRequest(Guid PlayerId, bool IsAdmin);
public record SetGameStateRequest(string State, string? Message);

public record GameStateResponse(string State, string? Message, DateTime UpdatedAt);

public record CreateItemRequest(
    string Name,
    string Description,
    ItemType Type,
    int MagicBonus,
    int StrengthBonus,
    int WisdomBonus,
    int SpeedBonus,
    long Price);
