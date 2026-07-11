namespace FraudDetection.API.Features.Auth
{
    public enum RoleUtilisateur
    {
        Analyste,
        Admin
    }

    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasseHash { get; set; } = string.Empty;
        public RoleUtilisateur Role { get; set; } = RoleUtilisateur.Analyste;
        public bool Actif { get; set; } = true;
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        public DateTime? DerniereConnexion { get; set; }

        // Navigation
        public ICollection<Log>? Logs { get; set; }

        public string NomComplet => $"{Prenom} {Nom}".Trim();
    }
}