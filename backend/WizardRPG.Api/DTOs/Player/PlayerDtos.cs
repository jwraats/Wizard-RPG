namespace WizardRPG.Api.DTOs.Player;

public record PlayerProfileResponse(
    Guid Id,
    string Username,
    string Email,
    long GoldCoins,
    int Level,
    long Experience,
    int MagicPower,
    int Strength,
    int Wisdom,
    int Speed,
    string ReferralCode,
    DateTime CreatedAt,
    bool IsAdmin);

public record UpdateProfileRequest(string? Username, string? Email);

public record AddExperienceRequest(long Amount);
