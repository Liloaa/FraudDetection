using FraudDetection.API.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.API.Features.Alertes
{
    public interface IAlerteService
    {
        Task<Alerte?> AnalyserEtCreerAlerte(int transactionId, float scoreRisque, string? raison);
        Task<List<Alerte>> ObtenirToutesAsync();
        Task<Alerte?> ObtenirParIdAsync(int id);
        Task<bool> ChangerStatutAsync(int alerteId, string nouveauStatut, int? utilisateurId);
    }

    public class AlerteService : IAlerteService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AlerteHub> _hubContext;

        // En dessous de ce seuil, on considère que ce n'est pas assez suspect
        private const float SEUIL_ALERTE = 0.5f;

        public AlerteService(AppDbContext context, IHubContext<AlerteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Alerte?> AnalyserEtCreerAlerte(int transactionId, float scoreRisque, string? raison)
        {
            // Sous le seuil : pas d'alerte créée
            if (scoreRisque < SEUIL_ALERTE)
                return null;

            var alerte = new Alerte
            {
                TransactionId = transactionId,
                ScoreRisque = scoreRisque,
                Raison = raison,
                Statut = "EnAttente",
                DetecteLe = DateTime.UtcNow
            };

            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            // Push temps réel vers tous les clients connectés au Hub
            await _hubContext.Clients.All.SendAsync("NouvelleAlerte", new
            {
                alerte.Id,
                alerte.TransactionId,
                alerte.ScoreRisque,
                alerte.Raison,
                alerte.Statut,
                alerte.DetecteLe
            });

            return alerte;
        }

        public async Task<List<Alerte>> ObtenirToutesAsync()
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .OrderByDescending(a => a.DetecteLe)
                .ToListAsync();
        }

        public async Task<Alerte?> ObtenirParIdAsync(int id)
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ChangerStatutAsync(int alerteId, string nouveauStatut, int? utilisateurId)
        {
            var alerte = await _context.Alertes.FindAsync(alerteId);
            if (alerte == null) return false;

            alerte.Statut = nouveauStatut; // ex: "Traitee", "Rejetee"
            alerte.UtilisateurId = utilisateurId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}