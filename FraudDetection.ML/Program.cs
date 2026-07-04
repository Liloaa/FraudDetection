using FraudDetection.ML;
using Microsoft.ML;

Console.WriteLine("=== Démarrage de l'entraînement du modèle de détection de fraude ===\n");

ModelTrainer.TrainAndSaveModel();

Console.WriteLine("\n=== Terminé ===\n");

// ----- TEST : simuler ce que fera l'API plus tard -----
Console.WriteLine("=== Test du modèle sur des transactions inventées ===\n");

var mlContext = new MLContext();

// On recharge le modèle qu'on vient de sauvegarder, exactement comme
// le fera FraudDetectionService dans l'API.
ITransformer model = mlContext.Model.Load("fraud_model.zip", out var schema);
var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionData, FraudPrediction>(model);

// Cas 1 : une transaction qui ressemble clairement à une fraude
// (montant élevé, heure de nuit, fréquence élevée, gros écart)
var transactionSuspecte = new TransactionData
{
    Montant = 850000,
    HeureTransaction = 2.5f,
    FrequenceTransactions24h = 11,
    EcartMontantMoyen = 700000
};
var resultat1 = predictionEngine.Predict(transactionSuspecte);
Console.WriteLine($"Transaction SUSPECTE -> Fraude prédite : {resultat1.EstFraudePredite} " +
                   $"(probabilité : {resultat1.Probability:P1})");

// Cas 2 : une transaction clairement normale
// (petit montant, heure de journée, peu de fréquence, écart faible)
var transactionNormale = new TransactionData
{
    Montant = 25000,
    HeureTransaction = 14.0f,
    FrequenceTransactions24h = 2,
    EcartMontantMoyen = 3000
};
var resultat2 = predictionEngine.Predict(transactionNormale);
Console.WriteLine($"Transaction NORMALE  -> Fraude prédite : {resultat2.EstFraudePredite} " +
                   $"(probabilité : {resultat2.Probability:P1})");

// Cas 3 : une transaction ambiguë (montant moyen-élevé mais heure normale)
// pour voir comment le modèle réagit dans un cas moins évident
var transactionAmbigue = new TransactionData
{
    Montant = 180000,
    HeureTransaction = 15.0f,
    FrequenceTransactions24h = 4,
    EcartMontantMoyen = 60000
};
var resultat3 = predictionEngine.Predict(transactionAmbigue);
Console.WriteLine($"Transaction AMBIGUË  -> Fraude prédite : {resultat3.EstFraudePredite} " +
                   $"(probabilité : {resultat3.Probability:P1})");

// Cas 4 : montant élevé mais tout le reste est normal (heure, fréquence, écart faible)
var cas4 = new TransactionData
{
    Montant = 280000,
    HeureTransaction = 11.0f,
    FrequenceTransactions24h = 2,
    EcartMontantMoyen = 15000
};
var r4 = predictionEngine.Predict(cas4);
Console.WriteLine($"Cas 4 (montant seul elevé)     -> Fraude : {r4.EstFraudePredite} ({r4.Probability:P1})");

// Cas 5 : montant faible mais heure de nuit + fréquence élevée
var cas5 = new TransactionData
{
    Montant = 45000,
    HeureTransaction = 2.0f,
    FrequenceTransactions24h = 10,
    EcartMontantMoyen = 30000
};
var r5 = predictionEngine.Predict(cas5);
Console.WriteLine($"Cas 5 (heure+frequence seules) -> Fraude : {r5.EstFraudePredite} ({r5.Probability:P1})");

// Cas 6 : très proche de la frontière qu'on avait dans le dataset (~150-200k, écart 50-70%)
var cas6 = new TransactionData
{
    Montant = 160000,
    HeureTransaction = 13.0f,
    FrequenceTransactions24h = 5,
    EcartMontantMoyen = 90000
};
var r6 = predictionEngine.Predict(cas6);
Console.WriteLine($"Cas 6 (proche frontiere)       -> Fraude : {r6.EstFraudePredite} ({r6.Probability:P1})");

// Cas 7 : montant très élevé mais tous les autres signaux normaux
var cas7 = new TransactionData
{
    Montant = 500000,
    HeureTransaction = 10.0f,
    FrequenceTransactions24h = 1,
    EcartMontantMoyen = 10000
};
var r7 = predictionEngine.Predict(cas7);
Console.WriteLine($"Cas 7 (gros montant isolé)     -> Fraude : {r7.EstFraudePredite} ({r7.Probability:P1})");