using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Bank;
using WizardRPG.Api.Models;
using WizardRPG.Api.Services;
using Xunit;

namespace WizardRPG.Tests.Unit.Services;

public class BankServiceTests
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
        Username = "TestWizard",
        Email = "test@wizard.com",
        PasswordHash = "hash",
        GoldCoins = gold,
        ReferralCode = "TESTCODE"
    };

    [Fact]
    public async Task DepositAsync_ValidAmount_DecreasesPlayerGold()
    {
        using var db = CreateInMemoryContext(nameof(DepositAsync_ValidAmount_DecreasesPlayerGold));
        var player = CreatePlayer(500);
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new BankService(db);
        var result = await service.DepositAsync(player.Id, 200);

        Assert.Equal(200, result.GoldBalance);
        Assert.Equal(300, player.GoldCoins);
    }

    [Fact]
    public async Task DepositAsync_InsufficientGold_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(DepositAsync_InsufficientGold_ThrowsInvalidOperationException));
        var player = CreatePlayer(100);
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new BankService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DepositAsync(player.Id, 500));
    }

    [Fact]
    public async Task DepositAsync_ZeroAmount_ThrowsArgumentException()
    {
        using var db = CreateInMemoryContext(nameof(DepositAsync_ZeroAmount_ThrowsArgumentException));
        var player = CreatePlayer();
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new BankService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.DepositAsync(player.Id, 0));
    }

    [Fact]
    public async Task WithdrawAsync_ValidAmount_IncreasesPlayerGold()
    {
        using var db = CreateInMemoryContext(nameof(WithdrawAsync_ValidAmount_IncreasesPlayerGold));
        var player = CreatePlayer(200);
        db.Players.Add(player);
        var account = new BankAccount { PlayerId = player.Id, GoldBalance = 500 };
        db.BankAccounts.Add(account);
        await db.SaveChangesAsync();

        var service = new BankService(db);
        var result = await service.WithdrawAsync(player.Id, 300);

        Assert.Equal(200, result.GoldBalance);
        Assert.Equal(500, player.GoldCoins);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientBalance_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext(nameof(WithdrawAsync_InsufficientBalance_ThrowsInvalidOperationException));
        var player = CreatePlayer();
        db.Players.Add(player);
        var account = new BankAccount { PlayerId = player.Id, GoldBalance = 50 };
        db.BankAccounts.Add(account);
        await db.SaveChangesAsync();

        var service = new BankService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.WithdrawAsync(player.Id, 100));
    }

    [Fact]
    public async Task GetAccountAsync_NoExistingAccount_CreatesOne()
    {
        using var db = CreateInMemoryContext(nameof(GetAccountAsync_NoExistingAccount_CreatesOne));
        var player = CreatePlayer();
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new BankService(db);
        var result = await service.GetAccountAsync(player.Id);

        Assert.Equal(player.Id, result.PlayerId);
        Assert.Equal(0, result.GoldBalance);
    }

    [Fact]
    public async Task StoreItemAsync_ValidItem_CreatesBankItem()
    {
        using var db = CreateInMemoryContext(nameof(StoreItemAsync_ValidItem_CreatesBankItem));
        var player = CreatePlayer(1000);
        var item = new Item { Name = "Test Wand", Description = "A test wand", Type = ItemType.Wand, Price = 100 };
        db.Players.Add(player);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var service = new BankService(db);
        var result = await service.StoreItemAsync(player.Id, item.Id);

        Assert.Equal(item.Id, result.ItemId);
        Assert.Equal("Test Wand", result.ItemName);
    }
}
