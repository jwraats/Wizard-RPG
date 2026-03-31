namespace WizardRPG.Api.DTOs.House;

public record HouseLeaderboardEntry(string House, long TotalPoints, int MemberCount);
public record HousePointsResponse(Guid Id, Guid PlayerId, string PlayerUsername, string House, int Points, string Activity, DateTime EarnedAt);
public record SelectHouseRequest(string House);
public record AwardHousePointsRequest(Guid PlayerId, int Points, string Activity);
