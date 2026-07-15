using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FraudDetection.API.Features.Alertes;

namespace FraudDetection.API.Pages
{
    [Authorize]
    public class AlertesModel : PageModel
    {
        private readonly IAlerteService _alertes;

        public AlertesModel(IAlerteService alertes)
        {
            _alertes = alertes;
        }

        public List<Alerte> Alertes { get; set; } = new();

        public async Task OnGetAsync()
        {
            Alertes = await _alertes.GetToutesAsync();
        }

        public async Task<IActionResult> OnPostChangerStatutAsync(int id, string statut)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var utilisateurId))
                return Unauthorized();

            var ok = await _alertes.ChangerStatutAsync(id, statut, utilisateurId);
            return ok ? new JsonResult(new { succes = true }) : NotFound();
        }
    }
}