using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.WizardChess;

public record ChessMatchResponse(
    Guid Id, Guid ChallengerId, string ChallengerUsername,
    Guid? DefenderId, string? DefenderUsername,
    Guid? WinnerId, string? WinnerUsername,
    ChessMatchStatus Status, long BetAmount,
    string BoardState, bool IsPlayerTurn, int TurnCount,
    DateTime CreatedAt, DateTime? FinishedAt);

public record CreateChessMatchRequest(Guid? DefenderId, long BetAmount);

public record ChessMoveRequest(int FromRow, int FromCol, int ToRow, int ToCol, bool UseAbility);

public record ChessMoveResponse(
    bool Valid, string Narrative, string BoardState,
    bool IsPlayerTurn, bool GameOver, Guid? WinnerId, string? WinnerUsername);
