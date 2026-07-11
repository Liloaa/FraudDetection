using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.API.Features.Rapports
{
    [ApiController]
    [Route("api/rapports")]
    [Authorize]
    public class RapportsController : ControllerBase
    {
        private readonly IStatistiquesService _statistiques;

        public RapportsController(IStatistiquesService statistiques)
        {
            _statistiques = statistiques;
        }

        // GET /api/rapports/resume
        [HttpGet("resume")]
        public async Task<IActionResult> Resume()
        {
            return Ok(await _statistiques.GetResumeAsync());
        }

        // GET /api/rapports/par-jour?jours=7
        [HttpGet("par-jour")]
        public async Task<IActionResult> ParJour([FromQuery] int jours = 7)
        {
            return Ok(await _statistiques.GetTransactionsParJourAsync(jours));
        }

        // GET /api/rapports/par-lieu
        [HttpGet("par-lieu")]
        public async Task<IActionResult> ParLieu()
        {
            return Ok(await _statistiques.GetRepartitionParLieuAsync());
        }
    }
}