namespace WizardRPG.Api.Models;

public class FellowshipMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FellowshipId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public decimal ContributionPercent { get; set; } = 0;

    public Fellowship? Fellowship { get; set; }
    public Player? Player { get; set; }
}
