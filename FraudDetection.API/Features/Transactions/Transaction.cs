// Features/Transactions/Transaction.cs
namespace FraudDetection.API.Features.Transactions
{
    public class Transaction
    {
        public int Id { get; set; }
        public int CompteId { get; set; }
        public decimal Montant { get; set; }
        public string? Lieu { get; set; }
        public string Type { get; set; } = "Virement";

        // Normal | Suspect | Confirme
        public string Statut { get; set; } = "Normal";
        public DateTime DateTransaction { get; set; } = DateTime.UtcNow;

        // Navigation
        public Shared.Compte? Compte { get; set; }
        public Alertes.Alerte? Alerte { get; set; }
    }
}