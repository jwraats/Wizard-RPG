using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Auth;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(Guid playerId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Players.AnyAsync(p => p.Email == request.Email))
            throw new InvalidOperationException("Email already registered.");

        if (await _db.Players.AnyAsync(p => p.Username == request.Username))
            throw new InvalidOperationException("Username already taken.");

        var player = new Player
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            ReferralCode = GenerateReferralCode(),
            CreatedAt = DateTime.UtcNow,
            GoldCoins = 100
        };

        _db.Players.Add(player);

        var bankAccount = new BankAccount { PlayerId = player.Id };
        _db.BankAccounts.Add(bankAccount);

        // Handle referral
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var fellowship = await _db.Fellowships
                .FirstOrDefaultAsync(f => f.ReferralCode == request.ReferralCode);
            if (fellowship != null)
            {
                _db.FellowshipMembers.Add(new FellowshipMember
                {
                    FellowshipId = fellowship.Id,
                    PlayerId = player.Id,
                    ContributionPercent = 0
                });
            }
        }

        await _db.SaveChangesAsync();
        return await GenerateAuthResponseAsync(player);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateAuthResponseAsync(player);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.RefreshToken == refreshToken && p.RefreshTokenExpiry > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return await GenerateAuthResponseAsync(player);
    }

    public async Task RevokeTokenAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");
        player.RefreshToken = null;
        player.RefreshTokenExpiry = null;
        await _db.SaveChangesAsync();
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(Player player)
    {
        var accessToken = GenerateJwtToken(player);
        var refreshToken = GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        player.RefreshToken = refreshToken;
        player.RefreshTokenExpiry = expiry;
        await _db.SaveChangesAsync();

        var jwtExpiry = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60"));

        return new AuthResponse(
            accessToken,
            refreshToken,
            jwtExpiry,
            new PlayerAuthInfo(player.Id, player.Username, player.Email, player.IsAdmin));
    }

    private string GenerateJwtToken(Player player)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured.")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, player.Email),
            new Claim("username", player.Username),
            new Claim("isAdmin", player.IsAdmin.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateReferralCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
