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
    public int EloRating { get; set; } = 1000;
    public string House { get; set; } = string.Empty;
    public Guid? ReferredByPlayerId { get; set; }
    public int ReferralCount { get; set; } = 0;
    public DateTime? LastLoginDate { get; set; }
    public int LoginStreak { get; set; } = 0;
    public DateTime? LastLoginRewardDate { get; set; }
    public bool HasCompletedOnboarding { get; set; } = false;

    // Equipment slots
    public Guid? EquippedWandId { get; set; }
    public Guid? EquippedRobeId { get; set; }
    public Guid? EquippedHatId { get; set; }
    public Guid? EquippedAmuletId { get; set; }
    public Guid? EquippedBroomId { get; set; }

    // Navigation properties for equipped items
    public BankItem? EquippedWand { get; set; }
    public BankItem? EquippedRobe { get; set; }
    public BankItem? EquippedHat { get; set; }
    public BankItem? EquippedAmulet { get; set; }
    public BankItem? EquippedBroom { get; set; }
    public Player? ReferredByPlayer { get; set; }

    public BankAccount? BankAccount { get; set; }
    public ICollection<BankItem> BankItems { get; set; } = new List<BankItem>();
    public ICollection<BroomBet> BroomBets { get; set; } = new List<BroomBet>();
    public ICollection<FellowshipMember> FellowshipMemberships { get; set; } = new List<FellowshipMember>();
    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    public ICollection<Quest> Quests { get; set; } = new List<Quest>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<HousePoints> HousePoints { get; set; } = new List<HousePoints>();
}
