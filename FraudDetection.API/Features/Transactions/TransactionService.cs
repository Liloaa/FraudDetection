using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Shared;

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

        public TransactionService(AppDbContext context)
        {
            _context = context;
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

        public async Task<Transaction> CreerAsync(Transaction transaction)
        {
            transaction.DateTransaction = DateTime.Now;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
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
            var lignes = new List<string>();

            // Ignorer la première ligne (en-têtes)
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var ligne = await reader.ReadLineAsync();
                if (ligne == null) continue;

                var colonnes = ligne.Split(',');
                // Format CSV attendu :
                // CompteId, Montant, Lieu, Type, DateTransaction
                if (colonnes.Length < 5) continue;

                _context.Transactions.Add(new Transaction
                {
                    CompteId = int.Parse(colonnes[0]),
                    Montant = decimal.Parse(colonnes[1]),
                    Lieu = colonnes[2],
                    Type = colonnes[3],
                    DateTransaction = DateTime.Parse(colonnes[4]),
                    Statut = "Normal"
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}