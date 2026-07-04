using Microsoft.ML.Data;

namespace FraudDetection.ML
{
    public class FraudPrediction
    {
        // La prédiction du modèle : true = fraude probable, false = normale
        // ML.NET remplit automatiquement cette propriété après analyse
        [ColumnName("PredictedLabel")]
        public bool EstFraudePredite { get; set; }

        // Le degré de confiance du modèle dans sa réponse, entre 0 et 1
        // Ex: 0.95 = le modèle est très sûr, 0.51 = c'est limite/incertain
        public float Probability { get; set; }

        // Un score brut interne utilisé par l'algorithme (moins utile pour toi
        // directement, mais ML.NET l'ajoute automatiquement, donc on le garde)
        public float Score { get; set; }
    }
}