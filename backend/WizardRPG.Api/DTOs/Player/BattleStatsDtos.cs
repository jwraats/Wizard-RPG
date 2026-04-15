namespace WizardRPG.Api.DTOs.Player;

public record BattleStatsResponse(
    int TotalBattles, int Wins, int Losses, double WinRate,
    long TotalDamageDealt, long TotalDamageReceived,
    string? MostUsedSpell, int CurrentWinStreak, int BestWinStreak);
