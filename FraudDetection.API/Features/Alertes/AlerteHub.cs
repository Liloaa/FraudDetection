using Microsoft.AspNetCore.SignalR;

namespace FraudDetection.API.Features.Alertes
{
    public class AlerteHub : Hub
    {
        private const string GroupeAlertes = "Alertes";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupeAlertes);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupeAlertes);
            await base.OnDisconnectedAsync(exception);
        }
    }
}