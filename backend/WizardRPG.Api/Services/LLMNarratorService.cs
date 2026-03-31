namespace WizardRPG.Api.Services;

public interface ILLMNarratorService
{
    string GenerateTurnNarrative(string attackerName, string defenderName, string spellName, int damage);
    string GenerateBattleStory(List<string> turnNarratives);
}

public class LLMNarratorService : ILLMNarratorService
{
    private static readonly string[] TurnTemplates =
    [
        "{attacker} unleashes {spell} upon {defender}, dealing {damage} damage!",
        "With a flick of their wand, {attacker} casts {spell} at {defender} for {damage} damage!",
        "{attacker} channels arcane energy into {spell}, striking {defender} for {damage} damage!",
        "The air crackles as {attacker} sends {spell} hurtling toward {defender} — {damage} damage dealt!",
        "{defender} recoils as {attacker}'s {spell} lands for {damage} damage!",
    ];

    private static readonly string[] BattleIntros =
    [
        "In an epic clash of magical might, two wizards faced each other across the enchanted arena.",
        "The stars aligned for a legendary duel as two powerful mages prepared to test their skills.",
        "Thunder rolled as ancient magic stirred — a great battle was about to begin.",
    ];

    private static readonly string[] BattleOutros =
    [
        "As the dust settled, only one wizard remained standing.",
        "The battle had been fierce, but the victor had proven their worth.",
        "The crowd roared as the last spell faded into silence.",
    ];

    public string GenerateTurnNarrative(string attackerName, string defenderName, string spellName, int damage)
    {
        var rng = new Random();
        var template = TurnTemplates[rng.Next(TurnTemplates.Length)];
        return template
            .Replace("{attacker}", attackerName)
            .Replace("{defender}", defenderName)
            .Replace("{spell}", spellName)
            .Replace("{damage}", damage.ToString());
    }

    public string GenerateBattleStory(List<string> turnNarratives)
    {
        var rng = new Random();
        var intro = BattleIntros[rng.Next(BattleIntros.Length)];
        var outro = BattleOutros[rng.Next(BattleOutros.Length)];
        var body = string.Join(" ", turnNarratives);
        return $"{intro} {body} {outro}";
    }
}
