namespace WizardRPG.Api.Models;

public class BankItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public Guid ItemId { get; set; }
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    public Player? Player { get; set; }
    public Item? Item { get; set; }
}
