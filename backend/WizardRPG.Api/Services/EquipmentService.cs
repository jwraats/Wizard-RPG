using Microsoft.EntityFrameworkCore;
using WizardRPG.Api.Data;
using WizardRPG.Api.DTOs.Equipment;
using WizardRPG.Api.Models;

namespace WizardRPG.Api.Services;

public interface IEquipmentService
{
    Task<EquipmentSlots> GetEquipmentAsync(Guid playerId);
    Task<EquipmentSlots> EquipItemAsync(Guid playerId, Guid bankItemId);
    Task<EquipmentSlots> UnequipItemAsync(Guid playerId, string slot);
    Task<(int MagicBonus, int StrengthBonus, int WisdomBonus, int SpeedBonus)> GetEquipmentBonusesAsync(Guid playerId);
}

public class EquipmentService : IEquipmentService
{
    private readonly AppDbContext _db;

    public EquipmentService(AppDbContext db) => _db = db;

    public async Task<EquipmentSlots> GetEquipmentAsync(Guid playerId)
    {
        var player = await LoadPlayerWithEquipment(playerId);
        return MapToSlots(player);
    }

    public async Task<EquipmentSlots> EquipItemAsync(Guid playerId, Guid bankItemId)
    {
        var player = await LoadPlayerWithEquipment(playerId);

        var bankItem = await _db.BankItems
            .Include(bi => bi.Item)
            .FirstOrDefaultAsync(bi => bi.Id == bankItemId && bi.PlayerId == playerId)
            ?? throw new KeyNotFoundException("Bank item not found or does not belong to you.");

        var item = bankItem.Item
            ?? throw new InvalidOperationException("Item data not found.");

        switch (item.Type)
        {
            case ItemType.Wand:
                player.EquippedWandId = bankItem.Id;
                break;
            case ItemType.Robe:
                player.EquippedRobeId = bankItem.Id;
                break;
            case ItemType.Hat:
                player.EquippedHatId = bankItem.Id;
                break;
            case ItemType.Amulet:
                player.EquippedAmuletId = bankItem.Id;
                break;
            case ItemType.Broom:
                player.EquippedBroomId = bankItem.Id;
                break;
            default:
                throw new InvalidOperationException($"Item type '{item.Type}' cannot be equipped.");
        }

        await _db.SaveChangesAsync();
        player = await LoadPlayerWithEquipment(playerId);
        return MapToSlots(player);
    }

    public async Task<EquipmentSlots> UnequipItemAsync(Guid playerId, string slot)
    {
        var player = await _db.Players.FindAsync(playerId)
            ?? throw new KeyNotFoundException("Player not found.");

        switch (slot.ToLowerInvariant())
        {
            case "wand":
                player.EquippedWandId = null;
                break;
            case "robe":
                player.EquippedRobeId = null;
                break;
            case "hat":
                player.EquippedHatId = null;
                break;
            case "amulet":
                player.EquippedAmuletId = null;
                break;
            case "broom":
                player.EquippedBroomId = null;
                break;
            default:
                throw new ArgumentException($"Invalid slot '{slot}'. Must be one of: wand, robe, hat, amulet, broom.");
        }

        await _db.SaveChangesAsync();
        player = await LoadPlayerWithEquipment(playerId);
        return MapToSlots(player);
    }

    public async Task<(int MagicBonus, int StrengthBonus, int WisdomBonus, int SpeedBonus)> GetEquipmentBonusesAsync(Guid playerId)
    {
        var player = await LoadPlayerWithEquipment(playerId);

        int magic = 0, strength = 0, wisdom = 0, speed = 0;

        void AddBonuses(BankItem? bankItem)
        {
            if (bankItem?.Item == null) return;
            magic += bankItem.Item.MagicBonus;
            strength += bankItem.Item.StrengthBonus;
            wisdom += bankItem.Item.WisdomBonus;
            speed += bankItem.Item.SpeedBonus;
        }

        AddBonuses(player.EquippedWand);
        AddBonuses(player.EquippedRobe);
        AddBonuses(player.EquippedHat);
        AddBonuses(player.EquippedAmulet);
        AddBonuses(player.EquippedBroom);

        return (magic, strength, wisdom, speed);
    }

    private async Task<Player> LoadPlayerWithEquipment(Guid playerId)
    {
        return await _db.Players
            .Include(p => p.EquippedWand).ThenInclude(bi => bi!.Item)
            .Include(p => p.EquippedRobe).ThenInclude(bi => bi!.Item)
            .Include(p => p.EquippedHat).ThenInclude(bi => bi!.Item)
            .Include(p => p.EquippedAmulet).ThenInclude(bi => bi!.Item)
            .Include(p => p.EquippedBroom).ThenInclude(bi => bi!.Item)
            .FirstOrDefaultAsync(p => p.Id == playerId)
            ?? throw new KeyNotFoundException("Player not found.");
    }

    private static EquipmentSlots MapToSlots(Player player)
    {
        return new EquipmentSlots(
            MapItem(player.EquippedWand),
            MapItem(player.EquippedRobe),
            MapItem(player.EquippedHat),
            MapItem(player.EquippedAmulet),
            MapItem(player.EquippedBroom));
    }

    private static EquippedItemResponse? MapItem(BankItem? bankItem)
    {
        if (bankItem?.Item == null) return null;
        return new EquippedItemResponse(
            bankItem.Id, bankItem.ItemId, bankItem.Item.Name,
            bankItem.Item.Description, bankItem.Item.Type,
            bankItem.Item.MagicBonus, bankItem.Item.StrengthBonus,
            bankItem.Item.WisdomBonus, bankItem.Item.SpeedBonus);
    }
}
