using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.BroomGame;
using WizardRPG.Api.Models;
using WizardRPG.Api.Services;
using Xunit;

namespace WizardRPG.Tests.Unit.Services;

public class BroomGameServiceTests
{
    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static Player CreatePlayer(long gold = 1000) => new()
    {
        Id = Guid.NewGuid(),
        Username = "BettingWizard",
        Email = "bet@wizard.com",
        PasswordHash = "hash",
        GoldCoins = gold,
        ReferralCode = "BETCODE"
    };

    private static async Task<BroomLeague> CreateLeagueAsync(AppDbContext db, LeagueStatus status = LeagueStatus.Upcoming)
    {
        var league = new BroomLeague
        {
            Name = "Test Race",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            Status = status
        };
        var team1 = new BroomTeam { Name = "FireBrooms", Odds = 2.0m, LeagueId = league.Id };
        var team2 = new BroomTeam { Name = "IceBrooms", Odds = 3.5m, LeagueId = league.Id };
        league.Teams = [team1, team2];
        db.BroomLeagues.Add(league);
        await db.SaveChangesAsync();
        return league;
    }

    [Fact]
    public async Task PlaceBetAsync_ValidBet_DeductsGold()
    {
        using var db = CreateInMemoryContext(nameof(PlaceBetAsync_ValidBet_DeductsGold));
        var player = CreatePlayer(500);
        db.Players.Add(player);
        var league = await CreateLeagueAsync(db);
        var teamId = league.Teams.First().Id;

        var service = new BroomGameService(db);
        var result = await service.PlaceBetAsync(player.Id, new PlaceBetRequest(league.Id, teamId, 200));

        Assert.Equal(200, result.Amount);
        Assert.Equal(BetStatus.Pending, result.Status);
        Assert.Equal(300, player.GoldCoins);
    }

    [Fact]
    public async Task PlaceBetAsync_InsufficientGold_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(PlaceBetAsync_InsufficientGold_ThrowsInvalidOperationException));
        var player = CreatePlayer(50);
        db.Players.Add(player);
        var league = await CreateLeagueAsync(db);
        var teamId = league.Teams.First().Id;

        var service = new BroomGameService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PlaceBetAsync(player.Id, new PlaceBetRequest(league.Id, teamId, 200)));
    }

    [Fact]
    public async Task PlaceBetAsync_FinishedLeague_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(PlaceBetAsync_FinishedLeague_ThrowsInvalidOperationException));
        var player = CreatePlayer(500);
        db.Players.Add(player);
        var league = await CreateLeagueAsync(db, LeagueStatus.Finished);
        var teamId = league.Teams.First().Id;

        var service = new BroomGameService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PlaceBetAsync(player.Id, new PlaceBetRequest(league.Id, teamId, 100)));
    }

    [Fact]
    public async Task ResolveLeagueAsync_PayoutWinners()
    {
        using var db = CreateInMemoryContext(nameof(ResolveLeagueAsync_PayoutWinners));
        var player = CreatePlayer(1000);
        db.Players.Add(player);
        var league = await CreateLeagueAsync(db);
        var winnerTeam = league.Teams.First(t => t.Name == "FireBrooms");
        var loserTeam = league.Teams.First(t => t.Name == "IceBrooms");

        var bet = new BroomBet
        {
            PlayerId = player.Id,
            LeagueId = league.Id,
            TeamId = winnerTeam.Id,
            Amount = 100,
            Status = BetStatus.Pending
        };
        db.BroomBets.Add(bet);
        player.GoldCoins -= 100;
        await db.SaveChangesAsync();

        var service = new BroomGameService(db);
        await service.ResolveLeagueAsync(league.Id, winnerTeam.Id);

        Assert.Equal(BetStatus.Won, bet.Status);
        Assert.Equal(200, bet.Payout); // 100 * 2.0 odds
        Assert.Equal(1100, player.GoldCoins); // 900 + 200 payout
    }

    [Fact]
    public async Task ResolveLeagueAsync_AlreadyFinished_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(ResolveLeagueAsync_AlreadyFinished_ThrowsInvalidOperationException));
        var league = await CreateLeagueAsync(db, LeagueStatus.Finished);
        var teamId = league.Teams.First().Id;

        var service = new BroomGameService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ResolveLeagueAsync(league.Id, teamId));
    }

    [Fact]
    public async Task CreateLeagueAsync_LessThanTwoTeams_ThrowsArgumentException()
    {
        using var db = CreateInMemoryContext(nameof(CreateLeagueAsync_LessThanTwoTeams_ThrowsArgumentException));
        var service = new BroomGameService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateLeagueAsync(new CreateLeagueRequest(
                "Bad Race",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                [new CreateTeamRequest("OnlyTeam", 1.5m)])));
    }
}
