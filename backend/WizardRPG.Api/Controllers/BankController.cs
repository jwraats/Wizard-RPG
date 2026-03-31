using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WizardRPG.Api.DTOs.Bank;
using WizardRPG.Api.Services;

namespace WizardRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankController : ControllerBase
{
    private readonly IBankService _bankService;

    public BankController(IBankService bankService) => _bankService = bankService;

    private Guid CurrentPlayerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException());

    /// <summary>Get the current player's bank account.</summary>
    [HttpGet]
    public async Task<ActionResult<BankAccountResponse>> GetAccount()
    {
        var account = await _bankService.GetAccountAsync(CurrentPlayerId);
        return Ok(account);
    }

    /// <summary>Deposit gold coins from wallet to bank.</summary>
    [HttpPost("deposit")]
    public async Task<ActionResult<BankAccountResponse>> Deposit([FromBody] DepositRequest request)
    {
        var account = await _bankService.DepositAsync(CurrentPlayerId, request.Amount);
        return Ok(account);
    }

    /// <summary>Withdraw gold coins from bank to wallet.</summary>
    [HttpPost("withdraw")]
    public async Task<ActionResult<BankAccountResponse>> Withdraw([FromBody] WithdrawRequest request)
    {
        var account = await _bankService.WithdrawAsync(CurrentPlayerId, request.Amount);
        return Ok(account);
    }

    /// <summary>Get all items stored in the bank.</summary>
    [HttpGet("items")]
    public async Task<ActionResult<List<BankItemResponse>>> GetItems()
    {
        var items = await _bankService.GetItemsAsync(CurrentPlayerId);
        return Ok(items);
    }

    /// <summary>Store an item in the bank.</summary>
    [HttpPost("items")]
    public async Task<ActionResult<BankItemResponse>> StoreItem([FromBody] StoreItemRequest request)
    {
        var item = await _bankService.StoreItemAsync(CurrentPlayerId, request.ItemId);
        return Created($"api/bank/items/{item.Id}", item);
    }

    /// <summary>Retrieve an item from the bank.</summary>
    [HttpDelete("items/{bankItemId:guid}")]
    public async Task<IActionResult> RetrieveItem(Guid bankItemId)
    {
        await _bankService.RetrieveItemAsync(CurrentPlayerId, bankItemId);
        return NoContent();
    }
}
