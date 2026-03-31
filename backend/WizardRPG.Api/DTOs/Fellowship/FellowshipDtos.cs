namespace WizardRPG.Api.DTOs.Fellowship;

public record FellowshipResponse(
    Guid Id,
    string Name,
    Guid OwnerId,
    string OwnerUsername,
    string ReferralCode,
    long GoldPerHour,
    DateTime CreatedAt,
    List<FellowshipMemberResponse> Members);

public record FellowshipMemberResponse(
    Guid Id,
    Guid PlayerId,
    string Username,
    DateTime JoinedAt,
    decimal ContributionPercent);

public record CreateFellowshipRequest(string Name);
public record JoinFellowshipRequest(string ReferralCode);
public record UpdateContributionRequest(Guid MemberId, decimal ContributionPercent);
public record DistributeIncomeRequest(long TotalAmount);
