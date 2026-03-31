using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.Quiz;

public record QuizQuestionResponse(
    Guid Id,
    string QuestionText,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    QuizDifficulty Difficulty,
    QuizCategory Category);

public record SubmitQuizRequest(List<QuizAnswerRequest> Answers);
public record QuizAnswerRequest(Guid QuestionId, string SelectedOption);

public record QuizResultResponse(
    Guid Id,
    int Score,
    int TotalQuestions,
    long GoldEarned,
    int XpEarned,
    DateTime CompletedAt,
    List<QuizAnswerResult> AnswerResults);

public record QuizAnswerResult(Guid QuestionId, string SelectedOption, string CorrectOption, bool IsCorrect);

public record QuizLeaderboardEntry(string Username, int TotalScore, int QuizzesCompleted);
