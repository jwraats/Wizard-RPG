namespace WizardRPG.Api.Models;

public enum LeagueStatus
{
    Upcoming,
    Running,
    Finished
}

public class BroomLeague
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LeagueStatus Status { get; set; } = LeagueStatus.Upcoming;
    public Guid? WinnerTeamId { get; set; }

    public ICollection<BroomTeam> Teams { get; set; } = new List<BroomTeam>();
    public ICollection<BroomBet> Bets { get; set; } = new List<BroomBet>();
    public BroomTeam? WinnerTeam { get; set; }
}

public class BroomTeam
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Odds { get; set; } = 1.0m;
    public Guid LeagueId { get; set; }

    public BroomLeague? League { get; set; }
    public ICollection<BroomBet> Bets { get; set; } = new List<BroomBet>();
}
