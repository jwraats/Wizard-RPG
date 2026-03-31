namespace WizardRPG.Api.Services;

public interface ILLMNarratorService
{
    Task<string> GenerateTurnNarrativeAsync(string attackerName, string defenderName, string spellName, int damage);
    Task<string> GenerateBattleStoryAsync(List<string> turnNarratives);
}

/// <summary>
/// Placeholder implementation that returns simple formatted text.
/// Replace with a real LLM provider (OpenAI, Ollama, etc.) by implementing ILLMNarratorService.
/// </summary>
public class PlaceholderNarratorService : ILLMNarratorService
{
    public Task<string> GenerateTurnNarrativeAsync(string attackerName, string defenderName, string spellName, int damage)
    {
        var narrative = $"{attackerName} casts {spellName} on {defenderName} for {damage} damage!";
        return Task.FromResult(narrative);
    }

    public Task<string> GenerateBattleStoryAsync(List<string> turnNarratives)
    {
        var story = string.Join(" ", turnNarratives);
        return Task.FromResult(story);
    }
}
