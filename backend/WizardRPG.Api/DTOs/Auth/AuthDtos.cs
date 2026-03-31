namespace WizardRPG.Api.DTOs.Auth;

public record RegisterRequest(string Username, string Email, string Password, string? ReferralCode);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    PlayerAuthInfo Player);

public record PlayerAuthInfo(
    Guid Id,
    string Username,
    string Email,
    bool IsAdmin);
