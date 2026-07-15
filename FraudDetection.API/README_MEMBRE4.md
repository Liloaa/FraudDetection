# Membre 4 — Auth + Pages Razor (livraison)

## Ce qui a été fait

### Auth (`Features/Auth/`)
- `Utilisateur.cs` — entité avec `RoleUtilisateur` (Admin / Analyste)
- `Log.cs` — traçabilité (connexion, déconnexion, changement de statut d'alerte...)
- `AuthViewModels.cs` — `LoginViewModel` / `InscriptionViewModel`
- `AuthController.cs` — contrôleur MVC classique : `/Auth/Login`, `/Auth/Inscription`, `/Auth/Logout`, `/Auth/AccessDenied`
  - Auth par **cookie** (pas d'ASP.NET Identity complet, trop lourd pour le besoin)
  - Mot de passe hashé avec `PasswordHasher<Utilisateur>` (namespace `Microsoft.AspNetCore.Identity`, fourni par le framework partagé — pas de package NuGet à ajouter a priori)
  - Chaque connexion/déconnexion écrit un `Log`

### Pages (`Pages/`)
- `Dashboard.cshtml(.cs)` — stats globales + dernières transactions + alertes en attente
- `Transactions.cshtml(.cs)` — filtre (statut/dates), import CSV, liste
- `Alertes.cshtml(.cs)` — liste + **SignalR temps réel** (nouvelles alertes, changement de statut sans reload)
- `Rapports.cshtml(.cs)` — stats + 2 graphiques Chart.js (par jour, par lieu)
- `Shared/_Layout.cshtml` — sidebar de navigation + infos utilisateur connecté
- Toutes les Pages sont protégées par `[Authorize]` (+ convention globale dans `Program.cs`)

### Stubs complétés (Membre 3 — à vérifier/enrichir)
Ces fichiers étaient vides, je les ai rendus **fonctionnels a minima** pour débloquer mes pages :
- `AlerteService.cs` — CRUD + push SignalR sur `IHubContext<AlerteHub>`
- `AlerteHub.cs` — groupe `"Alertes"`, events `NouvelleAlerte` / `AlerteMiseAJour`
- `StatistiquesService.cs` — résumé, stats par jour, stats par lieu
- `RapportsController.cs` — expose `/api/rapports/resume`, `/par-jour`, `/par-lieu`

**⚠️ Mariam / Membre 3 : n'hésite pas à modifier ces fichiers**, ils sont volontairement simples. Le score de fraude (`ScoreRisque`/`Raison`) dans `AlerteService.CreerDepuisTransactionAsync` est censé venir du module ML de Membre 1 — actuellement rien ne l'appelle automatiquement, il faudra brancher ça quand le modèle sera prêt.

### Program.cs
- Ajout `UseAuthentication()` (manquait) + config cookie
- `AddRazorPages(options => options.Conventions.AuthorizeFolder("/"))`
- DI : `IAlerteService`, `IStatistiquesService`, `IPasswordHasher<Utilisateur>`
- `app.MapHub<AlerteHub>("/hubs/alertes")`
- Seed d'un compte Admin par défaut si la table `Utilisateurs` est vide :
  - **email** : `admin@frauddetection.mg`
  - **mot de passe** : `Admin123!`
  - ⚠️ à changer avant toute démo publique

## Compte de test
| Email | Mot de passe | Rôle |
|---|---|---|
| admin@frauddetection.mg | Admin123! | Admin |

Vous pouvez aussi créer un compte via `/Auth/Inscription` (rôle Analyste par défaut).

## À vérifier avant de merger
1. **`.csproj`** : je n'avais pas ce fichier. `PasswordHasher<T>` devrait fonctionner nativement avec le SDK Web, mais si erreur de compilation, ajoutez le package `Microsoft.Extensions.Identity.Core`.
2. Les CSS/JS (`wwwroot/css/site.css`, `wwwroot/js/alertes.js`) sont nouveaux — pas de conflit avec l'existant (`signalr.js` déjà présent, réutilisé tel quel).
3. `Program.cs` : j'ai changé la route par défaut vers `Auth/Login` (au lieu de `Home/Index`) puisqu'il n'y a pas de HomeController dans le projet. Dites-moi si ça casse quelque chose côté Membre 2/3.
4. Migration EF : comme `Program.cs` utilise `EnsureCreated()` (pas de migrations), les nouvelles tables `Utilisateurs`/`Logs` seront créées automatiquement au premier lancement — si vous passez à `dotnet ef migrations`, pensez à générer une migration incluant `Utilisateur`/`Log`.

## Structure livrée
Tous les fichiers respectent l'arborescence donnée dans le brief (`Features/Auth`, `Pages/`, etc.). Copiez-collez simplement dans votre repo aux bons emplacements.