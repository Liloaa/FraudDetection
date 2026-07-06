using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Features.Transactions;
using FraudDetection.API.Features.Alertes;
using FraudDetection.API.Features.Auth;

namespace FraudDetection.API.Shared
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Tables — vous ajoutez ici quand les autres membres
        // créent leurs modèles
        public DbSet<Client> Clients { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Alerte> Alertes { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Transaction → une seule Alerte maximum
            modelBuilder.Entity<Alerte>()
                .HasOne(a => a.Transaction)
                .WithOne(t => t.Alerte)
                .HasForeignKey<Alerte>(a => a.TransactionId);

            // Index uniques
            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Compte>()
                .HasIndex(c => c.NumeroCompte).IsUnique();

            // Index de performance
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.DateTransaction);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Statut);

            base.OnModelCreating(modelBuilder);
        }
    }
}