using Microsoft.ML.Data;

namespace FraudDetection.ML
{
    public class TransactionData
    {
        // Le montant de la transaction, en Ariary (ou la devise utilisée)
        [LoadColumn(0)]
        public float Montant { get; set; }

        // L'heure de la transaction, sur 24h (ex: 3.5 = 3h30 du matin)
        [LoadColumn(1)]
        public float HeureTransaction { get; set; }

        // Nombre de transactions faites par ce client dans les dernières 24h
        [LoadColumn(2)]
        public float FrequenceTransactions24h { get; set; }

        // Différence entre cette transaction et la moyenne habituelle du client
        // Ex: si le client dépense en moyenne 50 000 Ar et fait une transaction de 500 000 Ar,
        // l'écart est énorme -> signal d'alerte
        [LoadColumn(3)]
        public float EcartMontantMoyen { get; set; }

        // La "réponse" : est-ce que cette transaction est une fraude ? (0 = non, 1 = oui)
        // C'est cette colonne que le modèle va apprendre à prédire
        [LoadColumn(4)]
        [ColumnName("Label")]
        public bool EstFraude { get; set; }
    }
}