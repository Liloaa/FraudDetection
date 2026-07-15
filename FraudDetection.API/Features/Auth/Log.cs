namespace FraudDetection.API.Features.Auth
{
    // Trace les actions sensibles : connexion, déconnexion,
    // changement de statut d'une alerte, import CSV, etc.
    public class Log
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }

        // Ex: "Connexion", "Deconnexion", "ChangementStatutAlerte", "ImportCsv"
        public string Action { get; set; } = string.Empty;

        // Ex: "Alerte #12 -> Confirme"
        public string? Details { get; set; }

        public string? AdresseIp { get; set; }
        public DateTime DateLog { get; set; } = DateTime.UtcNow;

        // Navigation
        public Utilisateur? Utilisateur { get; set; }
    }
}