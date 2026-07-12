using Microsoft.AspNetCore.Mvc;
using FraudDetection.API.Services;
using FraudDetection.API.Features.Alertes;

namespace FraudDetection.API.Features.Transactions
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;
        private readonly IFraudDetectionService _fraudService;
        private readonly IAlerteService _alerteService;

        public TransactionController(
            ITransactionService service,
            IFraudDetectionService fraudService,
            IAlerteService alerteService)
        {
            _service = service;
            _fraudService = fraudService;
            _alerteService = alerteService;
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

            // Analyse par le modèle IA
            var (score, raison) = await _fraudService.AnalyserAsync(created);

            // Si le score de risque dépasse 50%, on crée une alerte
            if (score >= 0.5f)
            {
                await _alerteService.CreerDepuisTransactionAsync(created.Id, score, raison);
            }

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