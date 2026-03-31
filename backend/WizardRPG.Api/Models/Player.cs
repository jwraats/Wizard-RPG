namespace WizardRPG.Api.Models;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public long GoldCoins { get; set; } = 0;
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public int MagicPower { get; set; } = 10;
    public int Strength { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Speed { get; set; } = 10;
    public string ReferralCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAdmin { get; set; } = false;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public BankAccount? BankAccount { get; set; }
    public ICollection<BankItem> BankItems { get; set; } = new List<BankItem>();
    public ICollection<BroomBet> BroomBets { get; set; } = new List<BroomBet>();
    public ICollection<FellowshipMember> FellowshipMemberships { get; set; } = new List<FellowshipMember>();
    public ICollection<BrewAttempt> BrewAttempts { get; set; } = new List<BrewAttempt>();
}
