using Microsoft.AspNetCore.SignalR;

namespace FraudDetection.API.Features.Alertes
{
<<<<<<< HEAD
    public class AlerteHub : Hub
    {
        // Vide pour l'instant : le serveur pousse les alertes via IHubContext
        // (voir AlerteService.cs), pas besoin de méthodes ici pour le moment.
=======
    // Hub SignalR : tous les clients connectés rejoignent le groupe "Alertes"
    // et reçoivent les évènements poussés par AlerteService
    // ("NouvelleAlerte", "AlerteMiseAJour" - voir AlerteService.cs).
    //
    // NOTE pour Membre 3 : si vous ajoutez des méthodes appelables
    // depuis le client JS (ex: un utilisateur qui "s'abonne" à un compte
    // précis), ajoutez-les ici. Le socket est déjà monté dans Program.cs
    // sur la route /hubs/alertes.
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
>>>>>>> c5dbbd7738ad86a4843a1ad0ef82a914beede8c0
    }
}