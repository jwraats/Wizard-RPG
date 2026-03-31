using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.BroomGame;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IBroomGameService
{
    Task<List<BroomLeagueResponse>> GetLeaguesAsync(LeagueStatus? status = null);
    Task<BroomLeagueResponse> GetLeagueAsync(Guid leagueId);
    Task<BroomLeagueResponse> CreateLeagueAsync(CreateLeagueRequest request);
    Task<BroomBetResponse> PlaceBetAsync(Guid playerId, PlaceBetRequest request);
    Task<List<BroomBetResponse>> GetPlayerBetsAsync(Guid playerId);
    Task<BroomLeagueResponse> ResolveLeagueAsync(Guid leagueId, Guid winnerTeamId);
}

public class BroomGameService : IBroomGameService
{
    private readonly AppDbContext _db;

    public BroomGameService(AppDbContext db) => _db = db;

    public async Task<List<BroomLeagueResponse>> GetLeaguesAsync(LeagueStatus? status = null)
    {
        var query = _db.BroomLeagues.Include(l => l.Teams).AsQueryable();
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var leagues = await query.OrderByDescending(l => l.StartTime).ToListAsync();
        return leagues.Select(MapLeagueToResponse).ToList();
    }

    public async Task<BroomLeagueResponse> GetLeagueAsync(Guid leagueId)
    {
        var league = await _db.BroomLeagues
            .Include(l => l.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId)
            ?? throw new KeyNotFoundException("League not found.");
        return MapLeagueToResponse(league);
    }

    public async Task<BroomLeagueResponse> CreateLeagueAsync(CreateLeagueRequest request)
    {
        if (request.Teams.Count < 2)
            throw new ArgumentException("A league must have at least 2 teams.");

        var league = new BroomLeague
        {
            Name = request.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = LeagueStatus.Upcoming
        };

        league.Teams = request.Teams.Select(t => new BroomTeam
        {
            Name = t.Name,
            Odds = t.Odds,
            LeagueId = league.Id
        }).ToList();

        _db.BroomLeagues.Add(league);
        await _db.SaveChangesAsync();
        return MapLeagueToResponse(league);
    }

    public async Task<BroomBetResponse> PlaceBetAsync(Guid playerId, PlaceBetRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Bet amount must be positive.");

        var league = await _db.BroomLeagues
            .Include(l => l.Teams)
            .FirstOrDefaultAsync(l => l.Id == request.LeagueId)
            ?? throw new KeyNotFoundException("League not found.");

        if (league.Status != LeagueStatus.Upcoming && league.Status != LeagueStatus.Running)
            throw new InvalidOperationException("Cannot place bets on a finished league.");

        var team = league.Teams.FirstOrDefault(t => t.Id == request.TeamId)
            ?? throw new KeyNotFoundException("Team not found in this league.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (player.GoldCoins < request.Amount)
            throw new InvalidOperationException("Insufficient gold coins.");

        player.GoldCoins -= request.Amount;

        var bet = new BroomBet
        {
            PlayerId = playerId,
            LeagueId = request.LeagueId,
            TeamId = request.TeamId,
            Amount = request.Amount,
            Status = BetStatus.Pending
        };

        _db.BroomBets.Add(bet);
        await _db.SaveChangesAsync();

        return new BroomBetResponse(
            bet.Id, league.Id, league.Name,
            team.Id, team.Name, bet.Amount,
            bet.Status, bet.Payout, bet.PlacedAt);
    }

    public async Task<List<BroomBetResponse>> GetPlayerBetsAsync(Guid playerId)
    {
        var bets = await _db.BroomBets
            .Include(b => b.League)
            .Include(b => b.Team)
            .Where(b => b.PlayerId == playerId)
            .OrderByDescending(b => b.PlacedAt)
            .ToListAsync();

        return bets.Select(b => new BroomBetResponse(
            b.Id, b.LeagueId, b.League!.Name,
            b.TeamId, b.Team!.Name, b.Amount,
            b.Status, b.Payout, b.PlacedAt)).ToList();
    }

    public async Task<BroomLeagueResponse> ResolveLeagueAsync(Guid leagueId, Guid winnerTeamId)
    {
        var league = await _db.BroomLeagues
            .Include(l => l.Teams)
            .Include(l => l.Bets)
            .ThenInclude(b => b.Player)
            .FirstOrDefaultAsync(l => l.Id == leagueId)
            ?? throw new KeyNotFoundException("League not found.");

        if (league.Status == LeagueStatus.Finished)
            throw new InvalidOperationException("League is already resolved.");

        if (league.Teams.All(t => t.Id != winnerTeamId))
            throw new KeyNotFoundException("Winner team not found in this league.");

        var winnerTeam = league.Teams.First(t => t.Id == winnerTeamId);

        league.Status = LeagueStatus.Finished;
        league.WinnerTeamId = winnerTeamId;

        foreach (var bet in league.Bets)
        {
            if (bet.TeamId == winnerTeamId)
            {
                bet.Status = BetStatus.Won;
                bet.Payout = (long)(bet.Amount * winnerTeam.Odds);
                if (bet.Player != null)
                    bet.Player.GoldCoins += bet.Payout;
            }
            else
            {
                bet.Status = BetStatus.Lost;
            }
        }

        await _db.SaveChangesAsync();
        return MapLeagueToResponse(league);
    }

    private static BroomLeagueResponse MapLeagueToResponse(BroomLeague l) => new(
        l.Id, l.Name, l.StartTime, l.EndTime, l.Status, l.WinnerTeamId,
        l.Teams.Select(t => new BroomTeamResponse(t.Id, t.Name, t.Odds)).ToList());
}
