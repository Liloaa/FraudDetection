using Microsoft.AspNetCore.SignalR;

namespace FraudDetection.API.Features.Alertes
{
    public class AlerteHub : Hub
    {
        // Vide pour l'instant : le serveur pousse les alertes via IHubContext
        // (voir AlerteService.cs), pas besoin de méthodes ici pour le moment.
    }
}