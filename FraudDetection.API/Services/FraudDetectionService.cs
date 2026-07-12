using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using FraudDetection.ML;
using FraudDetection.API.Shared;
using FraudDetection.API.Features.Transactions;

namespace FraudDetection.API.Services
{
    // L'interface : elle définit CE QUE le service peut faire, sans dire
    // COMMENT. Ça permet à d'autres parties du code (comme TransactionController)
    // de dépendre de cette interface plutôt que de la classe concrète -
    // une bonne pratique qui facilite les tests et les changements futurs.
    public interface IFraudDetectionService
    {
        Task<(float score, string raison)> AnalyserAsync(Transaction transaction);
    }

    public class FraudDetectionService : IFraudDetectionService
    {
        // Le "moteur" de prédiction, chargé une seule fois au démarrage.
        private readonly PredictionEngine<TransactionData, FraudPrediction> _predictionEngine;

        // On a besoin d'accéder à la base de données pour calculer l'historique
        // du compte (fréquence de transactions, montant moyen habituel).
        private readonly AppDbContext _context;

        // Le constructeur s'exécute UNE SEULE FOIS par requête (car le service
        // est enregistré en "Scoped" - une instance par requête HTTP).
        // On charge le modèle ici pour ne pas le recharger à chaque appel.
        public FraudDetectionService(AppDbContext context)
        {
            _context = context;

            var mlContext = new MLContext();

            // "Services/fraud_model.zip" car le chemin est relatif au dossier
            // où l'API s'exécute (bin/Debug/net8.0/), et le fichier y est copié
            // dans un sous-dossier Services/ grâce à notre configuration .csproj.
            ITransformer model = mlContext.Model.Load("Services/fraud_model.zip", out var schema);

            _predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionData, FraudPrediction>(model);
        }

        public async Task<(float score, string raison)> AnalyserAsync(Transaction transaction)
        {
            // ----- Calcul des 4 features à partir de l'historique -----

            // 1. HeureTransaction : l'heure en décimal (ex: 14h30 -> 14.5)
            float heure = transaction.DateTransaction.Hour + transaction.DateTransaction.Minute / 60f;

            // 2. FrequenceTransactions24h : nombre de transactions de ce compte
            // dans les 24h précédant CETTE transaction (pas après, sinon on
            // "trahirait" le futur - erreur classique à éviter en ML)
            var borneBasse = transaction.DateTransaction.AddHours(-24);
            int frequence = await _context.Transactions
                .Where(t => t.CompteId == transaction.CompteId
                         && t.DateTransaction >= borneBasse
                         && t.DateTransaction < transaction.DateTransaction)
                .CountAsync();

            // 3. EcartMontantMoyen : écart entre cette transaction et la moyenne
            // des transactions précédentes de ce même compte
            var transactionsPrecedentes = await _context.Transactions
                .Where(t => t.CompteId == transaction.CompteId
                         && t.DateTransaction < transaction.DateTransaction)
                .Select(t => t.Montant)
                .ToListAsync();

            float montantMoyen = transactionsPrecedentes.Any()
                ? (float)transactionsPrecedentes.Average()
                : (float)transaction.Montant; // pas d'historique = pas d'écart

            float ecart = (float)transaction.Montant - montantMoyen;

            // ----- Prédiction -----

            var donnees = new TransactionData
            {
                Montant = (float)transaction.Montant,
                HeureTransaction = heure,
                FrequenceTransactions24h = frequence,
                EcartMontantMoyen = ecart
            };

            var prediction = _predictionEngine.Predict(donnees);

            string raison = prediction.EstFraudePredite
                ? $"Montant: {transaction.Montant}, Heure: {heure:F1}h, " +
                  $"Frequence 24h: {frequence}, Ecart moyenne: {ecart:F0}"
                : "Transaction jugee normale";

            return (prediction.Probability, raison);
        }
    }
}