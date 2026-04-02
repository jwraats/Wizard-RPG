namespace WizardRPG.Api.Models;

public enum QuizDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum QuizCategory
{
    SpellLore,
    MagicalCreatures,
    PotionIngredients,
    WizardHistory
}

public class QuizQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = string.Empty; // "A", "B", "C", or "D"
    public QuizDifficulty Difficulty { get; set; }
    public QuizCategory Category { get; set; }
}

public class QuizAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public long GoldEarned { get; set; }
    public int XpEarned { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
}
