using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Admin;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IAdminService
{
    Task<List<AdminPlayerResponse>> GetAllPlayersAsync();
    Task<AdminPlayerResponse> GetPlayerAsync(Guid playerId);
    Task<AdminPlayerResponse> SetGoldAsync(SetGoldRequest request);
    Task<AdminPlayerResponse> SetAdminStatusAsync(SetAdminRequest request);
    Task GrantItemAsync(GrantItemRequest request);
    Task RevokeItemAsync(RevokeItemRequest request);
    Task<List<ItemResponse>> GetAllItemsAsync();
    Task<ItemResponse> CreateItemAsync(CreateItemRequest request);
    Task DeleteItemAsync(Guid itemId);
}

public record ItemResponse(
    Guid Id, string Name, string Description, ItemType Type,
    int MagicBonus, int StrengthBonus, int WisdomBonus, int SpeedBonus, long Price);

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<List<AdminPlayerResponse>> GetAllPlayersAsync()
    {
        var players = await _db.Players.OrderBy(p => p.Username).ToListAsync();
        return players.Select(MapToResponse).ToList();
    }

    public async Task<AdminPlayerResponse> GetPlayerAsync(Guid playerId)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");
        return MapToResponse(player);
    }

    public async Task<AdminPlayerResponse> SetGoldAsync(SetGoldRequest request)
    {
        var player = await _db.Players.FindAsync(request.PlayerId)
            ?? throw new KeyNotFoundException("Player not found.");
        player.GoldCoins = request.Amount;
        await _db.SaveChangesAsync();
        return MapToResponse(player);
    }

    public async Task<AdminPlayerResponse> SetAdminStatusAsync(SetAdminRequest request)
    {
        var player = await _db.Players.FindAsync(request.PlayerId)
            ?? throw new KeyNotFoundException("Player not found.");
        player.IsAdmin = request.IsAdmin;
        await _db.SaveChangesAsync();
        return MapToResponse(player);
    }

    public async Task GrantItemAsync(GrantItemRequest request)
    {
        var player = await _db.Players.FindAsync(request.PlayerId)
            ?? throw new KeyNotFoundException("Player not found.");

        var item = await _db.Items.FindAsync(request.ItemId)
            ?? throw new KeyNotFoundException("Item not found.");

        var bankItem = new BankItem
        {
            PlayerId = request.PlayerId,
            ItemId = request.ItemId,
            AcquiredAt = DateTime.UtcNow
        };
        _db.BankItems.Add(bankItem);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeItemAsync(RevokeItemRequest request)
    {
        var bankItem = await _db.BankItems.FindAsync(request.BankItemId)
            ?? throw new KeyNotFoundException("Bank item not found.");
        _db.BankItems.Remove(bankItem);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ItemResponse>> GetAllItemsAsync()
    {
        var items = await _db.Items.OrderBy(i => i.Name).ToListAsync();
        return items.Select(MapItemToResponse).ToList();
    }

    public async Task<ItemResponse> CreateItemAsync(CreateItemRequest request)
    {
        var item = new Item
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            MagicBonus = request.MagicBonus,
            StrengthBonus = request.StrengthBonus,
            WisdomBonus = request.WisdomBonus,
            SpeedBonus = request.SpeedBonus,
            Price = request.Price
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return MapItemToResponse(item);
    }

    public async Task DeleteItemAsync(Guid itemId)
    {
        var item = await _db.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
    }

    private static AdminPlayerResponse MapToResponse(Player p) => new(
        p.Id, p.Username, p.Email, p.GoldCoins, p.Level, p.Experience, p.IsAdmin, p.CreatedAt);

    private static ItemResponse MapItemToResponse(Item i) => new(
        i.Id, i.Name, i.Description, i.Type, i.MagicBonus, i.StrengthBonus, i.WisdomBonus, i.SpeedBonus, i.Price);
}
