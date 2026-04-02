using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Quiz;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IQuizService
{
    Task<List<QuizQuestionResponse>> GetRandomQuestionsAsync(int count = 5, QuizDifficulty? difficulty = null);
    Task<QuizResultResponse> SubmitQuizAsync(Guid playerId, SubmitQuizRequest request);
    Task<List<QuizResultResponse>> GetQuizHistoryAsync(Guid playerId);
    Task<List<QuizLeaderboardEntry>> GetLeaderboardAsync(int top = 10);
}

public class QuizService : IQuizService
{
    private readonly AppDbContext _db;

    public QuizService(AppDbContext db) => _db = db;

    public async Task<List<QuizQuestionResponse>> GetRandomQuestionsAsync(int count = 5, QuizDifficulty? difficulty = null)
    {
        var query = _db.QuizQuestions.AsQueryable();
        if (difficulty.HasValue)
            query = query.Where(q => q.Difficulty == difficulty.Value);

        var questions = await query
            .OrderBy(q => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return questions.Select(q => new QuizQuestionResponse(
            q.Id, q.QuestionText,
            q.OptionA, q.OptionB, q.OptionC, q.OptionD,
            q.Difficulty, q.Category)).ToList();
    }

    public async Task<QuizResultResponse> SubmitQuizAsync(Guid playerId, SubmitQuizRequest request)
    {
        if (request.Answers.Count == 0)
            throw new ArgumentException("No answers submitted.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
        var questions = await _db.QuizQuestions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var answerResults = new List<QuizAnswerResult>();
        int score = 0;
        long totalGold = 0;
        int totalXp = 0;

        foreach (var answer in request.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question))
                continue;

            bool isCorrect = string.Equals(answer.SelectedOption, question.CorrectOption, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                score++;
                totalGold += question.Difficulty switch
                {
                    QuizDifficulty.Easy => 10,
                    QuizDifficulty.Medium => 20,
                    QuizDifficulty.Hard => 30,
                    _ => 10
                };
                totalXp += question.Difficulty switch
                {
                    QuizDifficulty.Easy => 15,
                    QuizDifficulty.Medium => 25,
                    QuizDifficulty.Hard => 40,
                    _ => 15
                };
            }

            answerResults.Add(new QuizAnswerResult(
                question.Id, answer.SelectedOption, question.CorrectOption, isCorrect));
        }

        player.GoldCoins += totalGold;
        player.Experience += totalXp;

        var attempt = new QuizAttempt
        {
            PlayerId = playerId,
            Score = score,
            TotalQuestions = request.Answers.Count,
            GoldEarned = totalGold,
            XpEarned = totalXp
        };

        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        return new QuizResultResponse(
            attempt.Id, attempt.Score, attempt.TotalQuestions,
            attempt.GoldEarned, attempt.XpEarned, attempt.CompletedAt,
            answerResults);
    }

    public async Task<List<QuizResultResponse>> GetQuizHistoryAsync(Guid playerId)
    {
        var attempts = await _db.QuizAttempts
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.CompletedAt)
            .ToListAsync();

        return attempts.Select(a => new QuizResultResponse(
            a.Id, a.Score, a.TotalQuestions,
            a.GoldEarned, a.XpEarned, a.CompletedAt,
            new List<QuizAnswerResult>())).ToList();
    }

    public async Task<List<QuizLeaderboardEntry>> GetLeaderboardAsync(int top = 10)
    {
        var leaderboard = await _db.QuizAttempts
            .Include(a => a.Player)
            .GroupBy(a => new { a.PlayerId, a.Player!.Username })
            .Select(g => new QuizLeaderboardEntry(
                g.Key.Username,
                g.Sum(a => a.Score),
                g.Count()))
            .OrderByDescending(e => e.TotalScore)
            .Take(top)
            .ToListAsync();

        return leaderboard;
    }
}
