using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Fellowship;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IFellowshipService
{
    Task<FellowshipResponse> GetFellowshipAsync(Guid fellowshipId);
    Task<List<FellowshipResponse>> GetAllFellowshipsAsync();
    Task<FellowshipResponse?> GetPlayerFellowshipAsync(Guid playerId);
    Task<FellowshipResponse> CreateFellowshipAsync(Guid playerId, CreateFellowshipRequest request);
    Task<FellowshipResponse> JoinFellowshipAsync(Guid playerId, string referralCode);
    Task LeaveFellowshipAsync(Guid playerId, Guid fellowshipId);
    Task<FellowshipResponse> UpdateContributionAsync(Guid ownerId, Guid fellowshipId, UpdateContributionRequest request);
    Task DistributeIncomeAsync(Guid fellowshipId, long totalAmount);
}

public class FellowshipService : IFellowshipService
{
    private readonly AppDbContext _db;

    public FellowshipService(AppDbContext db) => _db = db;

    public async Task<FellowshipResponse> GetFellowshipAsync(Guid fellowshipId)
    {
        var fellowship = await LoadFellowshipAsync(fellowshipId)
            ?? throw new KeyNotFoundException("Fellowship not found.");
        return MapToResponse(fellowship);
    }

    public async Task<List<FellowshipResponse>> GetAllFellowshipsAsync()
    {
        var fellowships = await _db.Fellowships
            .Include(f => f.Owner)
            .Include(f => f.Members).ThenInclude(m => m.Player)
            .OrderByDescending(f => f.GoldPerHour)
            .ToListAsync();
        return fellowships.Select(MapToResponse).ToList();
    }

    public async Task<FellowshipResponse?> GetPlayerFellowshipAsync(Guid playerId)
    {
        var member = await _db.FellowshipMembers
            .Include(m => m.Fellowship).ThenInclude(f => f!.Owner)
            .Include(m => m.Fellowship).ThenInclude(f => f!.Members).ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(m => m.PlayerId == playerId);

        return member?.Fellowship != null ? MapToResponse(member.Fellowship) : null;
    }

    public async Task<FellowshipResponse> CreateFellowshipAsync(Guid playerId, CreateFellowshipRequest request)
    {
        var existing = await _db.FellowshipMembers.AnyAsync(m => m.PlayerId == playerId);
        if (existing)
            throw new InvalidOperationException("You are already a member of a fellowship.");

        var fellowship = new Fellowship
        {
            Name = request.Name,
            OwnerId = playerId,
            ReferralCode = GenerateReferralCode(),
            GoldPerHour = 0
        };

        var ownerMembership = new FellowshipMember
        {
            FellowshipId = fellowship.Id,
            PlayerId = playerId,
            ContributionPercent = 100
        };

        _db.Fellowships.Add(fellowship);
        _db.FellowshipMembers.Add(ownerMembership);
        await _db.SaveChangesAsync();

        return await GetFellowshipAsync(fellowship.Id);
    }

    public async Task<FellowshipResponse> JoinFellowshipAsync(Guid playerId, string referralCode)
    {
        var existing = await _db.FellowshipMembers.AnyAsync(m => m.PlayerId == playerId);
        if (existing)
            throw new InvalidOperationException("You are already a member of a fellowship.");

        var fellowship = await _db.Fellowships.FirstOrDefaultAsync(f => f.ReferralCode == referralCode)
            ?? throw new KeyNotFoundException("Fellowship with this referral code not found.");

        var membership = new FellowshipMember
        {
            FellowshipId = fellowship.Id,
            PlayerId = playerId,
            ContributionPercent = 0
        };

        _db.FellowshipMembers.Add(membership);
        await _db.SaveChangesAsync();

        return await GetFellowshipAsync(fellowship.Id);
    }

    public async Task LeaveFellowshipAsync(Guid playerId, Guid fellowshipId)
    {
        var fellowship = await LoadFellowshipAsync(fellowshipId)
            ?? throw new KeyNotFoundException("Fellowship not found.");

        if (fellowship.OwnerId == playerId)
            throw new InvalidOperationException("Owner cannot leave their own fellowship. Transfer ownership first.");

        var member = fellowship.Members.FirstOrDefault(m => m.PlayerId == playerId)
            ?? throw new KeyNotFoundException("You are not a member of this fellowship.");

        _db.FellowshipMembers.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task<FellowshipResponse> UpdateContributionAsync(Guid ownerId, Guid fellowshipId, UpdateContributionRequest request)
    {
        var fellowship = await LoadFellowshipAsync(fellowshipId)
            ?? throw new KeyNotFoundException("Fellowship not found.");

        if (fellowship.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Only the fellowship owner can update contributions.");

        var member = fellowship.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        if (request.ContributionPercent < 0 || request.ContributionPercent > 100)
            throw new ArgumentException("Contribution percent must be between 0 and 100.");

        member.ContributionPercent = request.ContributionPercent;
        await _db.SaveChangesAsync();
        return MapToResponse(fellowship);
    }

    public async Task DistributeIncomeAsync(Guid fellowshipId, long totalAmount)
    {
        var fellowship = await LoadFellowshipAsync(fellowshipId)
            ?? throw new KeyNotFoundException("Fellowship not found.");

        if (totalAmount <= 0) throw new ArgumentException("Amount must be positive.");

        foreach (var member in fellowship.Members.Where(m => m.ContributionPercent > 0))
        {
            var share = (long)(totalAmount * (double)member.ContributionPercent / 100.0);
            if (member.Player != null)
                member.Player.GoldCoins += share;
        }

        fellowship.GoldPerHour = totalAmount;
        await _db.SaveChangesAsync();
    }

    private async Task<Fellowship?> LoadFellowshipAsync(Guid fellowshipId) =>
        await _db.Fellowships
            .Include(f => f.Owner)
            .Include(f => f.Members).ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(f => f.Id == fellowshipId);

    private static string GenerateReferralCode() =>
        Guid.NewGuid().ToString("N")[..10].ToUpper();

    private static FellowshipResponse MapToResponse(Fellowship f) => new(
        f.Id, f.Name, f.OwnerId, f.Owner?.Username ?? string.Empty,
        f.ReferralCode, f.GoldPerHour, f.CreatedAt,
        f.Members.Select(m => new FellowshipMemberResponse(
            m.Id, m.PlayerId, m.Player?.Username ?? string.Empty,
            m.JoinedAt, m.ContributionPercent)).ToList());
}
