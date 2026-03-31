namespace WizardRPG.Api.Models;

public class Fellowship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public long GoldPerHour { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Player? Owner { get; set; }
    public ICollection<FellowshipMember> Members { get; set; } = new List<FellowshipMember>();
}
