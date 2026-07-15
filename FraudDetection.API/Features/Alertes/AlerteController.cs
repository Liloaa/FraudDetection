using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.API.Features.Alertes
{
    [ApiController]
    [Route("api/alertes")]
    public class AlerteController : ControllerBase
    {
        private readonly IAlerteService _alerteService;

        public AlerteController(IAlerteService alerteService)
        {
            _alerteService = alerteService;
        }

        // GET /api/alertes
        [HttpGet]
        public async Task<IActionResult> ObtenirToutes()
        {
            var alertes = await _alerteService.ObtenirToutesAsync();
            return Ok(alertes);
        }

        // GET /api/alertes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var alerte = await _alerteService.ObtenirParIdAsync(id);
            if (alerte == null) return NotFound();
            return Ok(alerte);
        }

        // PUT /api/alertes/{id}/statut
        [HttpPut("{id}/statut")]
        public async Task<IActionResult> ChangerStatut(int id, [FromBody] ChangerStatutRequest request)
        {
            var succes = await _alerteService.ChangerStatutAsync(id, request.Statut, request.UtilisateurId);
            if (!succes) return NotFound();
            return NoContent();
        }
    }

    // Petit objet pour recevoir le body du PUT proprement
    public class ChangerStatutRequest
    {
        public string Statut { get; set; } = string.Empty;
        public int? UtilisateurId { get; set; }
    }
}