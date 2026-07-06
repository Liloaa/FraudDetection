namespace FraudDetection.API.Features.Auth
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public DateTime CreeLe { get; set; } = DateTime.Now;
    }

    public class Log
    {
        public int Id { get; set; }
        public int AlerteId { get; set; }
        public int UtilisateurId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Commentaire { get; set; }
        public DateTime FaitLe { get; set; } = DateTime.Now;

        // Navigation
        public Alertes.Alerte? Alerte { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}