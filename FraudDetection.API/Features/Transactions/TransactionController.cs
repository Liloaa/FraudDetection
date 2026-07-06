using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.API.Features.Transactions
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        // GET /api/transactions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _service.GetAllAsync();
            return Ok(transactions);
        }

        // GET /api/transactions/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _service.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            return Ok(transaction);
        }

        // POST /api/transactions
        [HttpPost]
        public async Task<IActionResult> Creer([FromBody] Transaction transaction)
        {
            var created = await _service.CreerAsync(transaction);

            // POINT D'INTEGRATION avec Membre 1 (ML) et Membre 3 (Alertes)
            // Membre 1 livrera IFraudDetectionService → on l'appellera ici
            // Membre 3 livrera IAlerteService → on l'appellera ici aussi
            // Pour l'instant on laisse un commentaire TODO
            // TODO: var (score, raison) = _fraudService.Analyser(created, compte);
            // TODO: await _alerteService.AnalyserEtCreerAlerte(created, score, raison);

            return CreatedAtAction(nameof(GetById),
                new { id = created.Id }, created);
        }

        // GET /api/transactions/filtrer?statut=Suspect&dateDebut=2026-01-01
        [HttpGet("filtrer")]
        public async Task<IActionResult> Filtrer(
            [FromQuery] string? statut,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin)
        {
            var results = await _service.FiltrerAsync(statut, dateDebut, dateFin);
            return Ok(results);
        }

        // POST /api/transactions/importer
        [HttpPost("importer")]
        public async Task<IActionResult> ImporterCsv(IFormFile fichier)
        {
            if (fichier == null || fichier.Length == 0)
                return BadRequest("Fichier vide ou manquant");

            if (!fichier.FileName.EndsWith(".csv"))
                return BadRequest("Format attendu : .csv");

            await _service.ImporterCsvAsync(fichier);
            return Ok("Import réussi");
        }
    }
}