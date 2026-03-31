using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Quiz;
using WizardRPG.Api.Models;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService) => _quizService = quizService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get random quiz questions (correct answers not included).</summary>
    [HttpGet("questions")]
    [AllowAnonymous]
    public async Task<ActionResult<List<QuizQuestionResponse>>> GetQuestions(
        [FromQuery] int count = 5,
        [FromQuery] QuizDifficulty? difficulty = null)
    {
        var questions = await _quizService.GetRandomQuestionsAsync(count, difficulty);
        return Ok(questions);
    }

    /// <summary>Submit quiz answers and receive results.</summary>
    [HttpPost("submit")]
    public async Task<ActionResult<QuizResultResponse>> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        var result = await _quizService.SubmitQuizAsync(CurrentPlayerId, request);
        return Ok(result);
    }

    /// <summary>Get the current player's quiz history.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<QuizResultResponse>>> GetHistory()
    {
        var history = await _quizService.GetQuizHistoryAsync(CurrentPlayerId);
        return Ok(history);
    }

    /// <summary>Get the quiz leaderboard.</summary>
    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<ActionResult<List<QuizLeaderboardEntry>>> GetLeaderboard([FromQuery] int top = 10)
    {
        var leaderboard = await _quizService.GetLeaderboardAsync(top);
        return Ok(leaderboard);
    }
}
