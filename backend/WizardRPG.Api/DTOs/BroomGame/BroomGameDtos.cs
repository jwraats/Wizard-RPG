using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.BroomGame;

public record BroomLeagueResponse(
    Guid Id,
    string Name,
    DateTime StartTime,
    DateTime EndTime,
    LeagueStatus Status,
    Guid? WinnerTeamId,
    List<BroomTeamResponse> Teams);

public record BroomTeamResponse(Guid Id, string Name, decimal Odds);

public record CreateLeagueRequest(string Name, DateTime StartTime, DateTime EndTime, List<CreateTeamRequest> Teams);
public record CreateTeamRequest(string Name, decimal Odds);

public record PlaceBetRequest(Guid LeagueId, Guid TeamId, long Amount);

public record BroomBetResponse(
    Guid Id,
    Guid LeagueId,
    string LeagueName,
    Guid TeamId,
    string TeamName,
    long Amount,
    BetStatus Status,
    long Payout,
    DateTime PlacedAt);

public record ResolveLeagueRequest(Guid WinnerTeamId);
