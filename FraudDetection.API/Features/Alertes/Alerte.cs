namespace FraudDetection.API.Features.Alertes
{
    public class Alerte
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int? UtilisateurId { get; set; }
        public float ScoreRisque { get; set; }
        public string? Raison { get; set; }
        public string Statut { get; set; } = "EnAttente";
        public DateTime DetecteLe { get; set; } = DateTime.UtcNow;

        // Navigation
        public Features.Transactions.Transaction? Transaction { get; set; }
    }
}