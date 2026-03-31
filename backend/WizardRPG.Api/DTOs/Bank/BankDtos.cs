using WizardRPG.Api.Models;

namespace WizardRPG.Api.DTOs.Bank;

public record BankAccountResponse(Guid PlayerId, long GoldBalance, DateTime UpdatedAt);

public record DepositRequest(long Amount);
public record WithdrawRequest(long Amount);

public record BankItemResponse(
    Guid Id,
    Guid ItemId,
    string ItemName,
    string ItemDescription,
    ItemType ItemType,
    int MagicBonus,
    int StrengthBonus,
    int WisdomBonus,
    int SpeedBonus,
    DateTime AcquiredAt);

public record StoreItemRequest(Guid ItemId);
public record RetrieveItemRequest(Guid BankItemId);
