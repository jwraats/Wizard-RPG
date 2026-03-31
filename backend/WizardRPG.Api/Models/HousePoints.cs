namespace WizardRPG.Api.Models;

public class HousePoints
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string House { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Activity { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
}
