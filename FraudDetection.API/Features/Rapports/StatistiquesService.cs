using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Shared;

namespace FraudDetection.API.Features.Rapports
{
    public record ResumeStatistiques(
        int TotalTransactions,
        int TransactionsSuspectes,
        int TransactionsConfirmees,
        int AlertesEnAttente,
        double TauxFraudePourcent,
        decimal MontantTotalSuspect);

    public record StatJour(DateTime Jour, int NombreTransactions, int NombreSuspectes);

    public record StatLieu(string Lieu, int NombreTransactions, int NombreSuspectes);

    // NOTE pour Membre 3 : requêtes simples pour débloquer la page Rapports.
    // À enrichir avec vos propres indicateurs (par type de transaction,
    // par compte, etc.) selon vos besoins.
    public interface IStatistiquesService
    {
        Task<ResumeStatistiques> GetResumeAsync();
        Task<List<StatJour>> GetTransactionsParJourAsync(int jours);
        Task<List<StatLieu>> GetRepartitionParLieuAsync();
    }

    public class StatistiquesService : IStatistiquesService
    {
        private readonly AppDbContext _context;

        public StatistiquesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResumeStatistiques> GetResumeAsync()
        {
            var total = await _context.Transactions.CountAsync();
            var suspectes = await _context.Transactions.CountAsync(t => t.Statut == "Suspect");
            var confirmees = await _context.Transactions.CountAsync(t => t.Statut == "Confirme");
            var enAttente = await _context.Alertes.CountAsync(a => a.Statut == "EnAttente");
            var montantSuspect = await _context.Transactions
                .Where(t => t.Statut == "Suspect" || t.Statut == "Confirme")
                .SumAsync(t => (decimal?)t.Montant) ?? 0;

            var taux = total == 0 ? 0 : Math.Round((suspectes + confirmees) * 100.0 / total, 1);

            return new ResumeStatistiques(total, suspectes, confirmees, enAttente, taux, montantSuspect);
        }

        public async Task<List<StatJour>> GetTransactionsParJourAsync(int jours)
        {
            var depuis = DateTime.UtcNow.Date.AddDays(-Math.Max(jours, 1));

            var donnees = await _context.Transactions
                .Where(t => t.DateTransaction >= depuis)
                .GroupBy(t => t.DateTransaction.Date)
                .Select(g => new StatJour(
                    g.Key,
                    g.Count(),
                    g.Count(t => t.Statut == "Suspect" || t.Statut == "Confirme")))
                .OrderBy(s => s.Jour)
                .ToListAsync();

            return donnees;
        }

        public async Task<List<StatLieu>> GetRepartitionParLieuAsync()
        {
            return await _context.Transactions
                .Where(t => t.Lieu != null)
                .GroupBy(t => t.Lieu)
                .Select(g => new StatLieu(
                    g.Key!,
                    g.Count(),
                    g.Count(t => t.Statut == "Suspect" || t.Statut == "Confirme")))
                .OrderByDescending(s => s.NombreTransactions)
                .ToListAsync();
        }
    }
}