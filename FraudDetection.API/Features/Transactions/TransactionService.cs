using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Shared;
using FraudDetection.API.Services;
using FraudDetection.API.Features.Alertes;

namespace FraudDetection.API.Features.Transactions
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> CreerAsync(Transaction transaction);
        Task<List<Transaction>> FiltrerAsync(
            string? statut, DateTime? dateDebut, DateTime? dateFin);
        Task ImporterCsvAsync(IFormFile fichier);
    }

    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;
        // 🆕 On injecte les services nécessaires pour analyser CHAQUE transaction,
        // peu importe comment elle arrive (POST unique ou import CSV).
        private readonly IFraudDetectionService _fraudService;
        private readonly IAlerteService _alerteService;

        public TransactionService(
            AppDbContext context,
            IFraudDetectionService fraudService,
            IAlerteService alerteService)
        {
            _context = context;
            _fraudService = fraudService;
            _alerteService = alerteService;
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Compte)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(t => t.DateTransaction)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Compte)
                .ThenInclude(c => c!.Client)
                .Include(t => t.Alerte)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // 🆕 Méthode privée centrale : analyse une transaction et met à jour
        // son Statut + crée une Alerte si besoin. Utilisée par CreerAsync
        // ET par ImporterCsvAsync, pour garantir un comportement identique.
        private async Task AnalyserEtMettreAJourAsync(Transaction transaction)
        {
            var (score, raison) = await _fraudService.AnalyserAsync(transaction);

            transaction.Statut = score >= 0.5f ? "Suspect" : "Normal";

            if (score >= 0.5f)
            {
                await _alerteService.CreerDepuisTransactionAsync(transaction.Id, score, raison);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Transaction> CreerAsync(Transaction transaction)
        {
            // 🆕 On ignore volontairement le Statut envoyé par le client (sécurité) :
            // seul le serveur doit décider du statut, jamais l'utilisateur.
            transaction.Statut = "Normal";
            transaction.DateTransaction = DateTime.Now;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(); // Sauvegarde pour obtenir l'Id

            // 🆕 Analyse immédiate après création
            await AnalyserEtMettreAJourAsync(transaction);

            return transaction;
        }

        public async Task<List<Transaction>> FiltrerAsync(
            string? statut, DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.Transactions
                .Include(t => t.Compte)
                .AsQueryable();
            if (!string.IsNullOrEmpty(statut))
                query = query.Where(t => t.Statut == statut);
            if (dateDebut.HasValue)
                query = query.Where(t => t.DateTransaction >= dateDebut);
            if (dateFin.HasValue)
                query = query.Where(t => t.DateTransaction <= dateFin);
            return await query.ToListAsync();
        }

        public async Task ImporterCsvAsync(IFormFile fichier)
        {
            using var reader = new StreamReader(fichier.OpenReadStream());
            var nouvellesTransactions = new List<Transaction>();

            await reader.ReadLineAsync(); // ignorer l'en-tête
            while (!reader.EndOfStream)
            {
                var ligne = await reader.ReadLineAsync();
                if (ligne == null) continue;
                var colonnes = ligne.Split(',');
                if (colonnes.Length < 5) continue;

                var transaction = new Transaction
                {
                    CompteId = int.Parse(colonnes[0]),
                    Montant = decimal.Parse(colonnes[1]),
                    Lieu = colonnes[2],
                    Type = colonnes[3],
                    DateTransaction = DateTime.Parse(colonnes[4]),
                    Statut = "Normal" // valeur temporaire, sera mise à jour ci-dessous
                };
                _context.Transactions.Add(transaction);
                nouvellesTransactions.Add(transaction);
            }

            // 🆕 On sauvegarde d'abord pour que chaque transaction ait un Id
            await _context.SaveChangesAsync();

            // 🆕 Puis on analyse CHAQUE transaction importée avec le modèle IA
            foreach (var transaction in nouvellesTransactions)
            {
                await AnalyserEtMettreAJourAsync(transaction);
            }
        }
    }
}