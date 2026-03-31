using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.DungeonCrawler;

public record DungeonRunResponse(
    Guid Id, int CurrentFloor, int CurrentHp, int MaxHp,
    long GoldCollected, int XpCollected, DungeonRunStatus Status,
    DateTime StartedAt, DateTime? EndedAt);

public record DungeonRoomResponse(
    RoomType Type, string Description, List<DungeonChoiceResponse> Choices);

public record DungeonChoiceResponse(string Id, string Text, string RiskLevel);

public record DungeonActionRequest(string ChoiceId);

public record DungeonActionResponse(
    string Narrative, int HpChange, long GoldChange, int XpChange,
    int CurrentHp, long TotalGold, int TotalXp, bool RunEnded,
    DungeonRoomResponse? NextRoom);
