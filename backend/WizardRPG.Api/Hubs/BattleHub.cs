using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WizardRPG.Api.Hubs;

[Authorize]
public class BattleHub : Hub
{
    public async Task JoinBattleGroup(string battleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"battle-{battleId}");
    }

    public async Task LeaveBattleGroup(string battleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"battle-{battleId}");
    }

    public override async Task OnConnectedAsync()
    {
        var playerId = Context.UserIdentifier;
        if (playerId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"player-{playerId}");
        await base.OnConnectedAsync();
    }
}
