using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WizardRPG.Api.Hubs;

[Authorize]
public class GameHub : Hub
{
    public async Task JoinLeagueGroup(string leagueId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"league-{leagueId}");
    }

    public async Task LeaveLeagueGroup(string leagueId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"league-{leagueId}");
    }

    public async Task JoinFellowshipGroup(string fellowshipId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"fellowship-{fellowshipId}");
    }

    public async Task LeaveFellowshipGroup(string fellowshipId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"fellowship-{fellowshipId}");
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}
