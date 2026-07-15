<<<<<<< HEAD
﻿using FraudDetection.API.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.API.Features.Alertes
{
    public interface IAlerteService
    {
        Task<Alerte?> AnalyserEtCreerAlerte(int transactionId, float scoreRisque, string? raison);
        Task<List<Alerte>> ObtenirToutesAsync();
        Task<Alerte?> ObtenirParIdAsync(int id);
        Task<bool> ChangerStatutAsync(int alerteId, string nouveauStatut, int? utilisateurId);
=======
﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Features.Auth;
using FraudDetection.API.Shared;

namespace FraudDetection.API.Features.Alertes
{
    // NOTE pour Membre 3 : implémentation minimale pour débloquer les pages
    // Razor de Membre 4. Le calcul du ScoreRisque / Raison viendra du
    // module FraudDetection.ML (Membre 1) — CreerDepuisTransactionAsync
    // accepte ces valeurs en paramètres, à appeler depuis votre pipeline
    // de détection quand il sera prêt.
    public interface IAlerteService
    {
        Task<List<Alerte>> GetToutesAsync();
        Task<List<Alerte>> GetEnAttenteAsync();
        Task<Alerte?> GetByIdAsync(int id);
        Task<Alerte> CreerDepuisTransactionAsync(int transactionId, float scoreRisque, string? raison);

        // utilisateurId : qui a fait le changement (pour la table Log,
        // qui dans ce schéma trace les actions sur une Alerte précise)
        Task<bool> ChangerStatutAsync(int id, string nouveauStatut, int utilisateurId);
>>>>>>> c5dbbd7738ad86a4843a1ad0ef82a914beede8c0
    }

    public class AlerteService : IAlerteService
    {
        private readonly AppDbContext _context;
<<<<<<< HEAD
        private readonly IHubContext<AlerteHub> _hubContext;

        // En dessous de ce seuil, on considère que ce n'est pas assez suspect
        private const float SEUIL_ALERTE = 0.5f;

        public AlerteService(AppDbContext context, IHubContext<AlerteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Alerte?> AnalyserEtCreerAlerte(int transactionId, float scoreRisque, string? raison)
        {
            // Sous le seuil : pas d'alerte créée
            if (scoreRisque < SEUIL_ALERTE)
                return null;

=======
        private readonly IHubContext<AlerteHub> _hub;

        public AlerteService(AppDbContext context, IHubContext<AlerteHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<List<Alerte>> GetToutesAsync()
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .ThenInclude(t => t!.Compte)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(a => a.DetecteLe)
                .ToListAsync();
        }

        public async Task<List<Alerte>> GetEnAttenteAsync()
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .Where(a => a.Statut == "EnAttente")
                .OrderByDescending(a => a.DetecteLe)
                .ToListAsync();
        }

        public async Task<Alerte?> GetByIdAsync(int id)
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .ThenInclude(t => t!.Compte)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Alerte> CreerDepuisTransactionAsync(int transactionId, float scoreRisque, string? raison)
        {
>>>>>>> c5dbbd7738ad86a4843a1ad0ef82a914beede8c0
            var alerte = new Alerte
            {
                TransactionId = transactionId,
                ScoreRisque = scoreRisque,
                Raison = raison,
<<<<<<< HEAD
                Statut = "EnAttente",
                DetecteLe = DateTime.UtcNow
            };

            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            // Push temps réel vers tous les clients connectés au Hub
            await _hubContext.Clients.All.SendAsync("NouvelleAlerte", new
=======
                Statut = "EnAttente"
            };
            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            await _hub.Clients.Group("Alertes").SendAsync("NouvelleAlerte", new
>>>>>>> c5dbbd7738ad86a4843a1ad0ef82a914beede8c0
            {
                alerte.Id,
                alerte.TransactionId,
                alerte.ScoreRisque,
                alerte.Raison,
                alerte.Statut,
                alerte.DetecteLe
            });

            return alerte;
        }

<<<<<<< HEAD
        public async Task<List<Alerte>> ObtenirToutesAsync()
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .OrderByDescending(a => a.DetecteLe)
                .ToListAsync();
        }

        public async Task<Alerte?> ObtenirParIdAsync(int id)
        {
            return await _context.Alertes
                .Include(a => a.Transaction)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ChangerStatutAsync(int alerteId, string nouveauStatut, int? utilisateurId)
        {
            var alerte = await _context.Alertes.FindAsync(alerteId);
            if (alerte == null) return false;

            alerte.Statut = nouveauStatut; // ex: "Traitee", "Rejetee"
            alerte.UtilisateurId = utilisateurId;
            await _context.SaveChangesAsync();
=======
        public async Task<bool> ChangerStatutAsync(int id, string nouveauStatut, int utilisateurId)
        {
            var alerte = await _context.Alertes.FindAsync(id);
            if (alerte == null) return false;

            var ancienStatut = alerte.Statut;
            alerte.Statut = nouveauStatut;

            _context.Logs.Add(new Log
            {
                UtilisateurId = utilisateurId,
                Action = "ChangementStatutAlerte",
                Details = $"Alerte #{alerte.Id}: {ancienStatut} -> {nouveauStatut}"
            });

            await _context.SaveChangesAsync();

            await _hub.Clients.Group("Alertes").SendAsync("AlerteMiseAJour", new
            {
                alerte.Id,
                alerte.Statut
            });

>>>>>>> c5dbbd7738ad86a4843a1ad0ef82a914beede8c0
            return true;
        }
    }
}