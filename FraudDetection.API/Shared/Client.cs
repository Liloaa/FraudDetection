namespace FraudDetection.API.Shared
{
    public class Client
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public DateTime CreeLe { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Compte>? Comptes { get; set; }
    }
}