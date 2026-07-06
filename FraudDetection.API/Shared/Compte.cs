using FraudDetection.API.Features.Transactions;

namespace FraudDetection.API.Shared
{
    public class Compte
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string NumeroCompte { get; set; } = string.Empty;
        public string Type { get; set; } = "Courant";
        public decimal SoldeMoyen { get; set; }
        public string LieuHabituel { get; set; } = string.Empty;
        public DateTime OuvertLe { get; set; } = DateTime.UtcNow;

        // Navigation
        public Client? Client { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}