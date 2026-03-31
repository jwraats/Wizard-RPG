namespace WizardRPG.Api.Models;

public enum ItemType
{
    Wand,
    Robe,
    Hat,
    Broom,
    Potion,
    Amulet
}

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public int MagicBonus { get; set; } = 0;
    public int StrengthBonus { get; set; } = 0;
    public int WisdomBonus { get; set; } = 0;
    public int SpeedBonus { get; set; } = 0;
    public long Price { get; set; } = 0;

    public ICollection<BankItem> BankItems { get; set; } = new List<BankItem>();
}
