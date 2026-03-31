using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Bank;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IBankService
{
    Task<BankAccountResponse> GetAccountAsync(Guid playerId);
    Task<BankAccountResponse> DepositAsync(Guid playerId, long amount);
    Task<BankAccountResponse> WithdrawAsync(Guid playerId, long amount);
    Task<List<BankItemResponse>> GetItemsAsync(Guid playerId);
    Task<BankItemResponse> StoreItemAsync(Guid playerId, Guid itemId);
    Task RetrieveItemAsync(Guid playerId, Guid bankItemId);
}

public class BankService : IBankService
{
    private readonly AppDbContext _db;

    public BankService(AppDbContext db) => _db = db;

    public async Task<BankAccountResponse> GetAccountAsync(Guid playerId)
    {
        var account = await GetOrCreateAccountAsync(playerId);
        return MapToResponse(account);
    }

    public async Task<BankAccountResponse> DepositAsync(Guid playerId, long amount)
    {
        if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (player.GoldCoins < amount)
            throw new InvalidOperationException("Insufficient gold coins.");

        var account = await GetOrCreateAccountAsync(playerId);
        player.GoldCoins -= amount;
        account.GoldBalance += amount;
        account.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(account);
    }

    public async Task<BankAccountResponse> WithdrawAsync(Guid playerId, long amount)
    {
        if (amount <= 0) throw new ArgumentException("Withdrawal amount must be positive.");

        var account = await GetOrCreateAccountAsync(playerId);
        if (account.GoldBalance < amount)
            throw new InvalidOperationException("Insufficient bank balance.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        account.GoldBalance -= amount;
        player.GoldCoins += amount;
        account.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(account);
    }

    public async Task<List<BankItemResponse>> GetItemsAsync(Guid playerId)
    {
        var items = await _db.BankItems
            .Include(bi => bi.Item)
            .Where(bi => bi.PlayerId == playerId)
            .ToListAsync();

        return items.Select(MapItemToResponse).ToList();
    }

    public async Task<BankItemResponse> StoreItemAsync(Guid playerId, Guid itemId)
    {
        var item = await _db.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        if (player.GoldCoins < item.Price)
            throw new InvalidOperationException("Insufficient gold to store this item.");

        var bankItem = new BankItem
        {
            PlayerId = playerId,
            ItemId = itemId,
            AcquiredAt = DateTime.UtcNow
        };

        _db.BankItems.Add(bankItem);
        await _db.SaveChangesAsync();

        bankItem.Item = item;
        return MapItemToResponse(bankItem);
    }

    public async Task RetrieveItemAsync(Guid playerId, Guid bankItemId)
    {
        var bankItem = await _db.BankItems
            .FirstOrDefaultAsync(bi => bi.Id == bankItemId && bi.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Bank item not found.");

        _db.BankItems.Remove(bankItem);
        await _db.SaveChangesAsync();
    }

    private async Task<BankAccount> GetOrCreateAccountAsync(Guid playerId)
    {
        var account = await _db.BankAccounts.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (account is null)
        {
            account = new BankAccount { PlayerId = playerId };
            _db.BankAccounts.Add(account);
            await _db.SaveChangesAsync();
        }
        return account;
    }

    private static BankAccountResponse MapToResponse(BankAccount a) =>
        new(a.PlayerId, a.GoldBalance, a.UpdatedAt);

    private static BankItemResponse MapItemToResponse(BankItem bi) => new(
        bi.Id,
        bi.ItemId,
        bi.Item!.Name,
        bi.Item.Description,
        bi.Item.Type,
        bi.Item.MagicBonus,
        bi.Item.StrengthBonus,
        bi.Item.WisdomBonus,
        bi.Item.SpeedBonus,
        bi.AcquiredAt);
}
