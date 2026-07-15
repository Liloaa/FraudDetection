using FraudDetection.API.Features.Alertes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FraudDetection.API.Pages
{
    public class AlertesModel : PageModel
    {
        private readonly IAlerteService _alerteService;

        public List<Alerte> Alertes { get; set; } = new();

        public AlertesModel(IAlerteService alerteService)
        {
            _alerteService = alerteService;
        }

        public async Task OnGetAsync()
        {
            Alertes = await _alerteService.ObtenirToutesAsync();
        }
    }
}