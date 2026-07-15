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

        [HttpGet]
        public async Task<IActionResult> ObtenirToutes()
        {
            var alertes = await _alerteService.GetToutesAsync();
            return Ok(alertes);
        }

        [HttpGet("en-attente")]
        public async Task<IActionResult> ObtenirEnAttente()
        {
            var alertes = await _alerteService.GetEnAttenteAsync();
            return Ok(alertes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenirParId(int id)
        {
            var alerte = await _alerteService.GetByIdAsync(id);
            if (alerte == null) return NotFound();
            return Ok(alerte);
        }

        [HttpPut("{id}/statut")]
        public async Task<IActionResult> ChangerStatut(int id, [FromBody] ChangerStatutRequest request)
        {
            var succes = await _alerteService.ChangerStatutAsync(id, request.Statut, request.UtilisateurId);
            if (!succes) return NotFound();
            return NoContent();
        }
    }

    public class ChangerStatutRequest
    {
        public string Statut { get; set; } = string.Empty;
        public int UtilisateurId { get; set; }
    }
}