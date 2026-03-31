using Microsoft.EntityFrameworkCore;
using Moq;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Battle;
using WizardRPG.Api.Models;
using WizardRPG.Api.Services;
using Xunit;

namespace WizardRPG.Tests.Unit.Services;

public class BattleServiceTests
{
    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static Player CreatePlayer(string username, int magic = 20) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Email = $"{username}@wizard.com",
        PasswordHash = "hash",
        GoldCoins = 500,
        MagicPower = magic,
        Strength = 15,
        Wisdom = 15,
        Speed = 15,
        ReferralCode = Guid.NewGuid().ToString("N")[..8]
    };

    private static Spell CreateSpell(SpellElement element = SpellElement.Fire) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Spell",
        Description = "A test spell",
        ManaCost = 10,
        BaseDamage = 30,
        Effect = "Test",
        Element = element
    };

    private static Mock<ILLMNarratorService> CreateNarratorMock()
    {
        var mock = new Mock<ILLMNarratorService>();
        mock.Setup(n => n.GenerateTurnNarrativeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("A fierce spell was cast!");
        mock.Setup(n => n.GenerateBattleStoryAsync(It.IsAny<List<string>>()))
            .ReturnsAsync("An epic battle concluded.");
        return mock;
    }

    [Fact]
    public async Task ChallengeBattleAsync_CreatesPendingBattle()
    {
        using var db = CreateInMemoryContext(nameof(ChallengeBattleAsync_CreatesPendingBattle));
        var challenger = CreatePlayer("Merlin");
        var defender = CreatePlayer("Gandalf");
        db.Players.AddRange(challenger, defender);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);
        var result = await service.ChallengeBattleAsync(challenger.Id, defender.Id);

        Assert.Equal(BattleStatus.Pending, result.Status);
        Assert.Equal(challenger.Id, result.ChallengerId);
        Assert.Equal(defender.Id, result.DefenderId);
    }

    [Fact]
    public async Task ChallengeBattleAsync_SamePlayer_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(ChallengeBattleAsync_SamePlayer_ThrowsInvalidOperationException));
        var player = CreatePlayer("Merlin");
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ChallengeBattleAsync(player.Id, player.Id));
    }

    [Fact]
    public async Task AcceptBattleAsync_SetsStatusToActive()
    {
        using var db = CreateInMemoryContext(nameof(AcceptBattleAsync_SetsStatusToActive));
        var challenger = CreatePlayer("Merlin");
        var defender = CreatePlayer("Gandalf");
        db.Players.AddRange(challenger, defender);

        var battle = new Battle
        {
            ChallengerId = challenger.Id,
            DefenderId = defender.Id,
            Status = BattleStatus.Pending
        };
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);
        var result = await service.AcceptBattleAsync(battle.Id, defender.Id);

        Assert.Equal(BattleStatus.Active, result.Status);
    }

    [Fact]
    public async Task ExecuteTurnAsync_FirstTurn_ChallengerAttacks()
    {
        using var db = CreateInMemoryContext(nameof(ExecuteTurnAsync_FirstTurn_ChallengerAttacks));
        var challenger = CreatePlayer("Merlin");
        var defender = CreatePlayer("Gandalf");
        var spell = CreateSpell();
        db.Players.AddRange(challenger, defender);
        db.Spells.Add(spell);

        var battle = new Battle
        {
            ChallengerId = challenger.Id,
            DefenderId = defender.Id,
            Status = BattleStatus.Active
        };
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);
        var result = await service.ExecuteTurnAsync(battle.Id, challenger.Id, spell.Id);

        Assert.Single(result.Turns);
        Assert.Equal(challenger.Id, result.Turns[0].AttackerId);
    }

    [Fact]
    public async Task ExecuteTurnAsync_WrongPlayer_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(ExecuteTurnAsync_WrongPlayer_ThrowsInvalidOperationException));
        var challenger = CreatePlayer("Merlin");
        var defender = CreatePlayer("Gandalf");
        var spell = CreateSpell();
        db.Players.AddRange(challenger, defender);
        db.Spells.Add(spell);

        var battle = new Battle
        {
            ChallengerId = challenger.Id,
            DefenderId = defender.Id,
            Status = BattleStatus.Active
        };
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);

        // Defender tries to attack first (should be challenger's turn)
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteTurnAsync(battle.Id, defender.Id, spell.Id));
    }

    [Fact]
    public async Task ExecuteTurnAsync_TenTurns_BattleFinishes()
    {
        using var db = CreateInMemoryContext(nameof(ExecuteTurnAsync_TenTurns_BattleFinishes));
        var challenger = CreatePlayer("Merlin", magic: 50);
        var defender = CreatePlayer("Gandalf", magic: 30);
        var spell = CreateSpell();
        db.Players.AddRange(challenger, defender);
        db.Spells.Add(spell);

        var battle = new Battle
        {
            ChallengerId = challenger.Id,
            DefenderId = defender.Id,
            Status = BattleStatus.Active
        };
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var service = new BattleService(db, CreateNarratorMock().Object);

        BattleResponse? result = null;
        for (var i = 0; i < 10; i++)
        {
            var attackerId = i % 2 == 0 ? challenger.Id : defender.Id;
            result = await service.ExecuteTurnAsync(battle.Id, attackerId, spell.Id);
        }

        Assert.NotNull(result);
        Assert.Equal(BattleStatus.Finished, result.Status);
        Assert.NotNull(result.WinnerId);
    }
}
