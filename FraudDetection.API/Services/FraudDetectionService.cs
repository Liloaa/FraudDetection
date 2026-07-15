using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using FraudDetection.ML;
using FraudDetection.API.Shared;
using FraudDetection.API.Features.Transactions;

namespace FraudDetection.API.Services
{
    public interface IFraudDetectionService
    {
        Task<(float score, string raison)> AnalyserAsync(Transaction transaction);
    }

    public class FraudDetectionService : IFraudDetectionService
    {
        private readonly PredictionEngine<TransactionData, FraudPrediction> _predictionEngine;
        private readonly AppDbContext _context;

        // 🆕 Seuil au-delà duquel on considère l'écart comme "hors de portée"
        // du modèle ML.NET. Valeur provisoire : à ajuster une fois qu'on connaît
        // le montant max réel utilisé dans vos données d'entraînement.
        private const float SEUIL_ECART_EXTREME = 500000f;

        public FraudDetectionService(AppDbContext context)
        {
            _context = context;

            var mlContext = new MLContext();
            ITransformer model = mlContext.Model.Load("Services/fraud_model.zip", out var schema);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionData, FraudPrediction>(model);
        }

        public async Task<(float score, string raison)> AnalyserAsync(Transaction transaction)
        {
            float heure = transaction.DateTransaction.Hour + transaction.DateTransaction.Minute / 60f;

            var borneBasse = transaction.DateTransaction.AddHours(-24);
            int frequence = await _context.Transactions
                .Where(t => t.CompteId == transaction.CompteId
                         && t.DateTransaction >= borneBasse
                         && t.DateTransaction < transaction.DateTransaction)
                .CountAsync();

            var transactionsPrecedentes = await _context.Transactions
                .Where(t => t.CompteId == transaction.CompteId
                         && t.DateTransaction < transaction.DateTransaction)
                .Select(t => t.Montant)
                .ToListAsync();

            float montantMoyen = transactionsPrecedentes.Any()
                ? (float)transactionsPrecedentes.Average()
                : (float)transaction.Montant;

            float ecart = (float)transaction.Montant - montantMoyen;

            var donnees = new TransactionData
            {
                Montant = (float)transaction.Montant,
                HeureTransaction = heure,
                FrequenceTransactions24h = frequence,
                EcartMontantMoyen = ecart
            };

            var prediction = _predictionEngine.Predict(donnees);

            // 🆕 Garde-fou métier : le modèle ML.NET a été entraîné sur un
            // dataset limité et peut mal généraliser sur des écarts jamais vus
            // pendant l'entraînement (limite connue des arbres de décision type
            // FastTree, qui extrapolent mal hors de leur plage d'apprentissage).
            // On combine donc le ML avec une règle métier de sécurité, une
            // pratique standard dans les systèmes anti-fraude réels.
            bool ecartExtreme = Math.Abs(ecart) > SEUIL_ECART_EXTREME;
            bool estSuspecte = prediction.EstFraudePredite || ecartExtreme;

            // 🆕 On construit la raison différemment selon la source de la décision,
            // pour que ce soit traçable et explicable (important en conformité bancaire).
            string raison;
            if (ecartExtreme && !prediction.EstFraudePredite)
            {
                raison = $"Ecart extreme detecte (regle de securite) - " +
                          $"Montant: {transaction.Montant}, Ecart moyenne: {ecart:F0}, " +
                          $"Heure: {heure:F1}h, Frequence 24h: {frequence}";
            }
            else if (prediction.EstFraudePredite)
            {
                raison = $"Montant: {transaction.Montant}, Heure: {heure:F1}h, " +
                          $"Frequence 24h: {frequence}, Ecart moyenne: {ecart:F0}";
            }
            else
            {
                raison = "Transaction jugee normale";
            }

            // 🆕 On retourne un score de 1.0 (100%) quand c'est le garde-fou qui
            // déclenche, pour que ce soit visible clairement dans le dashboard.
            float score = ecartExtreme && !prediction.EstFraudePredite ? 1.0f : prediction.Probability;

            return (score, raison);
        }
    }
}