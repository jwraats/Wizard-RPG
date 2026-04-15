namespace WizardRPG.Api.DTOs.Equipment;

using WizardRPG.Api.Models;

public record EquipmentSlots(
    EquippedItemResponse? Wand,
    EquippedItemResponse? Robe,
    EquippedItemResponse? Hat,
    EquippedItemResponse? Amulet,
    EquippedItemResponse? Broom);

public record EquippedItemResponse(Guid BankItemId, Guid ItemId, string Name, string Description, ItemType Type, int MagicBonus, int StrengthBonus, int WisdomBonus, int SpeedBonus);

public record EquipItemRequest(Guid BankItemId);
public record UnequipItemRequest(string Slot);
