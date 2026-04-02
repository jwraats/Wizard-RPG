using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.WizardChess;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IWizardChessService
{
    Task<ChessMatchResponse> CreateMatchAsync(Guid playerId, CreateChessMatchRequest request);
    Task<ChessMatchResponse> GetMatchAsync(Guid matchId);
    Task<List<ChessMatchResponse>> GetPlayerMatchesAsync(Guid playerId);
    Task<ChessMoveResponse> MakeMoveAsync(Guid playerId, Guid matchId, ChessMoveRequest request);
    Task<ChessMatchResponse> ForfeitAsync(Guid playerId, Guid matchId);
}

public class WizardChessService : IWizardChessService
{
    private const int BoardSize = 6;
    private static readonly Random Rng = new();

    private readonly AppDbContext _db;

    public WizardChessService(AppDbContext db) => _db = db;

    public async Task<ChessMatchResponse> CreateMatchAsync(Guid playerId, CreateChessMatchRequest request)
    {
        if (request.BetAmount < 0)
            throw new ArgumentException("Bet amount cannot be negative.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (player.GoldCoins < request.BetAmount)
            throw new InvalidOperationException("Insufficient gold coins.");

        Player? defender = null;
        if (request.DefenderId.HasValue)
        {
            defender = await _db.Players.FindAsync(request.DefenderId.Value)
                ?? throw new KeyNotFoundException("Defender not found.");
        }

        player.GoldCoins -= request.BetAmount;

        var match = new ChessMatch
        {
            ChallengerId = playerId,
            DefenderId = request.DefenderId,
            BetAmount = request.BetAmount,
            Status = ChessMatchStatus.Active,
            BoardState = SerializeBoard(CreateInitialBoard()),
            IsPlayerTurn = true,
            TurnCount = 0
        };

        _db.ChessMatches.Add(match);
        await _db.SaveChangesAsync();

        return MapToResponse(match, player.Username, defender?.Username, null);
    }

    public async Task<ChessMatchResponse> GetMatchAsync(Guid matchId)
    {
        var match = await _db.ChessMatches
            .Include(m => m.Challenger)
            .Include(m => m.Defender)
            .Include(m => m.Winner)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match not found.");

        return MapToResponse(match,
            match.Challenger!.Username,
            match.Defender?.Username,
            match.Winner?.Username);
    }

    public async Task<List<ChessMatchResponse>> GetPlayerMatchesAsync(Guid playerId)
    {
        var matches = await _db.ChessMatches
            .Include(m => m.Challenger)
            .Include(m => m.Defender)
            .Include(m => m.Winner)
            .Where(m => m.ChallengerId == playerId || m.DefenderId == playerId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return matches.Select(m => MapToResponse(m,
            m.Challenger!.Username,
            m.Defender?.Username,
            m.Winner?.Username)).ToList();
    }

    public async Task<ChessMoveResponse> MakeMoveAsync(Guid playerId, Guid matchId, ChessMoveRequest request)
    {
        var match = await _db.ChessMatches
            .Include(m => m.Challenger)
            .Include(m => m.Defender)
            .Include(m => m.Winner)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match not found.");

        if (match.Status != ChessMatchStatus.Active)
            throw new InvalidOperationException("Match is not active.");

        bool isPvE = match.DefenderId == null;
        bool isChallenger = match.ChallengerId == playerId;
        bool isDefender = match.DefenderId == playerId;

        if (!isChallenger && !isDefender)
            throw new InvalidOperationException("You are not a participant in this match.");

        if (isPvE && !isChallenger)
            throw new InvalidOperationException("You are not a participant in this match.");

        // In PvE, only challenger moves. In PvP, check whose turn it is.
        if (isPvE && !match.IsPlayerTurn)
            throw new InvalidOperationException("It is not your turn.");
        if (!isPvE && match.IsPlayerTurn && !isChallenger)
            throw new InvalidOperationException("It is not your turn.");
        if (!isPvE && !match.IsPlayerTurn && !isDefender)
            throw new InvalidOperationException("It is not your turn.");

        var board = DeserializeBoard(match.BoardState);

        // Determine which side the current player controls
        bool playsLowercase = isPvE ? true : isChallenger;

        if (!IsValidMove(board, request.FromRow, request.FromCol, request.ToRow, request.ToCol, playsLowercase, request.UseAbility))
            return new ChessMoveResponse(false, "Invalid move.", match.BoardState, match.IsPlayerTurn, false, null, null);

        string capturedPiece = board[request.ToRow][request.ToCol];
        board[request.ToRow][request.ToCol] = board[request.FromRow][request.FromCol];
        board[request.FromRow][request.FromCol] = ".";
        match.TurnCount++;

        string narrative = $"Moved {PieceName(board[request.ToRow][request.ToCol])} from ({request.FromRow},{request.FromCol}) to ({request.ToRow},{request.ToCol}).";

        if (capturedPiece != ".")
            narrative += $" Captured {PieceName(capturedPiece)}!";

        // Check if opponent King was captured
        bool gameOver = false;
        Guid? winnerId = null;
        string? winnerUsername = null;

        if (capturedPiece == "K") // Opponent (uppercase) King captured by player (lowercase)
        {
            gameOver = true;
            winnerId = playsLowercase ? match.ChallengerId : match.DefenderId;
        }
        else if (capturedPiece == "k") // Player (lowercase) King captured by opponent (uppercase)
        {
            gameOver = true;
            winnerId = playsLowercase ? match.DefenderId : match.ChallengerId;
            if (isPvE) winnerId = null; // AI wins, no player winner
        }

        if (gameOver)
        {
            match.Status = ChessMatchStatus.Finished;
            match.FinishedAt = DateTime.UtcNow;
            match.WinnerId = winnerId;
            match.BoardState = SerializeBoard(board);
            await AwardWinningsAsync(match);
            await _db.SaveChangesAsync();

            winnerUsername = winnerId.HasValue
                ? (await _db.Players.FindAsync(winnerId.Value))?.Username
                : "AI";

            narrative += winnerId.HasValue ? $" {winnerUsername} wins!" : " The AI wins!";

            return new ChessMoveResponse(true, narrative, match.BoardState, match.IsPlayerTurn, true, winnerId, winnerUsername);
        }

        // PvE: AI makes a move after the player
        if (isPvE)
        {
            match.IsPlayerTurn = false;
            var aiNarrative = MakeAiMove(board);
            match.IsPlayerTurn = true;
            match.TurnCount++;
            narrative += " " + aiNarrative;

            // Check if AI captured player's King
            if (!BoardContains(board, "k"))
            {
                gameOver = true;
                match.Status = ChessMatchStatus.Finished;
                match.FinishedAt = DateTime.UtcNow;
                match.WinnerId = null; // AI won
                match.BoardState = SerializeBoard(board);
                await _db.SaveChangesAsync();
                narrative += " The AI wins!";
                return new ChessMoveResponse(true, narrative, match.BoardState, true, true, null, "AI");
            }

            // Check if player captured AI King during the player's move (already handled above)
        }
        else
        {
            match.IsPlayerTurn = !match.IsPlayerTurn;
        }

        match.BoardState = SerializeBoard(board);
        await _db.SaveChangesAsync();

        return new ChessMoveResponse(true, narrative, match.BoardState, match.IsPlayerTurn, false, null, null);
    }

    public async Task<ChessMatchResponse> ForfeitAsync(Guid playerId, Guid matchId)
    {
        var match = await _db.ChessMatches
            .Include(m => m.Challenger)
            .Include(m => m.Defender)
            .Include(m => m.Winner)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException("Match not found.");

        if (match.Status != ChessMatchStatus.Active)
            throw new InvalidOperationException("Match is not active.");

        bool isChallenger = match.ChallengerId == playerId;
        bool isDefender = match.DefenderId == playerId;
        if (!isChallenger && !isDefender)
            throw new InvalidOperationException("You are not a participant in this match.");

        match.Status = ChessMatchStatus.Forfeit;
        match.FinishedAt = DateTime.UtcNow;

        if (match.DefenderId == null)
        {
            // PvE forfeit - player loses bet
            match.WinnerId = null;
        }
        else
        {
            match.WinnerId = isChallenger ? match.DefenderId : match.ChallengerId;
            await AwardWinningsAsync(match);
        }

        await _db.SaveChangesAsync();

        return MapToResponse(match,
            match.Challenger!.Username,
            match.Defender?.Username,
            match.Winner?.Username);
    }

    // ── Board helpers ──────────────────────────────────────────────────

    private static string[][] CreateInitialBoard()
    {
        return new[]
        {
            new[] { "G", "N", "W", "K", "N", "G" },
            new[] { "P", "P", "P", "P", "P", "P" },
            new[] { ".", ".", ".", ".", ".", "." },
            new[] { ".", ".", ".", ".", ".", "." },
            new[] { "p", "p", "p", "p", "p", "p" },
            new[] { "g", "n", "w", "k", "n", "g" }
        };
    }

    private static string SerializeBoard(string[][] board) =>
        JsonSerializer.Serialize(board);

    private static string[][] DeserializeBoard(string boardState) =>
        JsonSerializer.Deserialize<string[][]>(boardState)
        ?? throw new InvalidOperationException("Invalid board state.");

    private static bool BoardContains(string[][] board, string piece)
    {
        for (int r = 0; r < BoardSize; r++)
            for (int c = 0; c < BoardSize; c++)
                if (board[r][c] == piece)
                    return true;
        return false;
    }

    // ── Move validation ────────────────────────────────────────────────

    private static bool IsValidMove(string[][] board, int fr, int fc, int tr, int tc, bool playsLowercase, bool useAbility)
    {
        if (fr < 0 || fr >= BoardSize || fc < 0 || fc >= BoardSize) return false;
        if (tr < 0 || tr >= BoardSize || tc < 0 || tc >= BoardSize) return false;
        if (fr == tr && fc == tc) return false;

        string piece = board[fr][fc];
        if (piece == ".") return false;

        bool isOwn = playsLowercase ? char.IsLower(piece[0]) : char.IsUpper(piece[0]);
        if (!isOwn) return false;

        string target = board[tr][tc];
        if (target != ".")
        {
            bool targetIsOwn = playsLowercase ? char.IsLower(target[0]) : char.IsUpper(target[0]);
            if (targetIsOwn) return false;

            // Golem ability: can't be captured from the front
            if (useAbility && (target == "G" || target == "g"))
            {
                int direction = playsLowercase ? -1 : 1; // lowercase moves "up" (decreasing row)
                if (tr - fr == direction && fc == tc) return false; // attacking from front
            }
        }

        string normalizedPiece = piece.ToUpperInvariant();
        int dr = tr - fr;
        int dc = tc - fc;

        return normalizedPiece switch
        {
            "P" => IsValidPawnMove(fr, fc, tr, tc, dr, dc, playsLowercase, board, target),
            "G" => IsValidGolemMove(dr, dc, board, fr, fc, tr, tc),
            "N" => IsValidKnightMove(dr, dc),
            "W" => IsValidWizardMove(dr, dc, board, fr, fc, tr, tc),
            "K" => IsValidKingMove(dr, dc),
            "B" => IsValidPhoenixMove(dr, dc, board, fr, fc, tr, tc),
            _ => false
        };
    }

    private static bool IsValidPawnMove(int fr, int fc, int tr, int tc, int dr, int dc, bool playsLowercase, string[][] board, string target)
    {
        int forward = playsLowercase ? -1 : 1;
        if (dc == 0 && dr == forward && target == ".")
            return true;
        if (Math.Abs(dc) == 1 && dr == forward && target != ".")
            return true;
        return false;
    }

    private static bool IsValidGolemMove(int dr, int dc, string[][] board, int fr, int fc, int tr, int tc)
    {
        if (dr != 0 && dc != 0) return false;
        return IsPathClear(board, fr, fc, tr, tc);
    }

    private static bool IsValidKnightMove(int dr, int dc)
    {
        int adr = Math.Abs(dr);
        int adc = Math.Abs(dc);
        return (adr == 2 && adc == 1) || (adr == 1 && adc == 2);
    }

    private static bool IsValidWizardMove(int dr, int dc, string[][] board, int fr, int fc, int tr, int tc)
    {
        if (dr == 0 || dc == 0)
            return IsPathClear(board, fr, fc, tr, tc);
        if (Math.Abs(dr) == Math.Abs(dc))
            return IsPathClear(board, fr, fc, tr, tc);
        return false;
    }

    private static bool IsValidKingMove(int dr, int dc)
    {
        return Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1;
    }

    private static bool IsPathClear(string[][] board, int fr, int fc, int tr, int tc)
    {
        int dr = Math.Sign(tr - fr);
        int dc = Math.Sign(tc - fc);
        int r = fr + dr;
        int c = fc + dc;
        while (r != tr || c != tc)
        {
            if (board[r][c] != ".") return false;
            r += dr;
            c += dc;
        }
        return true;
    }

    // Phoenix (B/b) uses bishop-like diagonal movement; not in starting lineup,
    // appears only via the Phoenix resurrection ability.

    private static bool IsValidPhoenixMove(int dr, int dc, string[][] board, int fr, int fc, int tr, int tc)
    {
        if (Math.Abs(dr) != Math.Abs(dc)) return false;
        return IsPathClear(board, fr, fc, tr, tc);
    }

    // ── AI moves ───────────────────────────────────────────────────────

    private static string MakeAiMove(string[][] board)
    {
        // AI plays uppercase pieces
        var validMoves = new List<(int fr, int fc, int tr, int tc)>();

        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                if (board[r][c] == "." || char.IsLower(board[r][c][0]))
                    continue;

                for (int tr = 0; tr < BoardSize; tr++)
                {
                    for (int tc = 0; tc < BoardSize; tc++)
                    {
                        if (IsValidMove(board, r, c, tr, tc, false, false))
                            validMoves.Add((r, c, tr, tc));
                    }
                }
            }
        }

        if (validMoves.Count == 0)
            return "AI has no valid moves.";

        var move = validMoves[Rng.Next(validMoves.Count)];
        string aiPiece = board[move.fr][move.fc];
        string captured = board[move.tr][move.tc];
        board[move.tr][move.tc] = board[move.fr][move.fc];
        board[move.fr][move.fc] = ".";

        string aiNarrative = $"AI moved {PieceName(aiPiece)} from ({move.fr},{move.fc}) to ({move.tr},{move.tc}).";
        if (captured != ".")
            aiNarrative += $" AI captured {PieceName(captured)}!";

        return aiNarrative;
    }

    // ── Gold management ────────────────────────────────────────────────

    private async Task AwardWinningsAsync(ChessMatch match)
    {
        if (match.WinnerId == null)
            return; // AI won or no winner

        var winner = await _db.Players.FindAsync(match.WinnerId.Value);
        if (winner == null) return;

        if (match.DefenderId == null)
        {
            // PvE: winner gets bet back + equal amount
            winner.GoldCoins += match.BetAmount * 2;
        }
        else
        {
            // PvP: winner gets double bet
            winner.GoldCoins += match.BetAmount * 2;
        }
    }

    // ── Mapping ────────────────────────────────────────────────────────

    private static ChessMatchResponse MapToResponse(ChessMatch m, string challengerUsername, string? defenderUsername, string? winnerUsername) =>
        new(m.Id, m.ChallengerId, challengerUsername,
            m.DefenderId, defenderUsername,
            m.WinnerId, winnerUsername,
            m.Status, m.BetAmount,
            m.BoardState, m.IsPlayerTurn, m.TurnCount,
            m.CreatedAt, m.FinishedAt);

    private static string PieceName(string piece) => piece.ToUpperInvariant() switch
    {
        "K" => "King",
        "W" => "Wizard",
        "N" => "Knight",
        "G" => "Golem",
        "B" => "Phoenix",
        "P" => "Pawn",
        _ => "Unknown"
    };
}
