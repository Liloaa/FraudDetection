using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FraudDetection.API.Shared;

namespace FraudDetection.API.Features.Auth
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Utilisateur> _hasher;

        public AuthController(AppDbContext context, IPasswordHasher<Utilisateur> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (utilisateur == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return View(model);
            }

            // Le champ s'appelle "MotDePasse" mais contient bien un hash
            // (produit par PasswordHasher<Utilisateur>) — jamais de mot
            // de passe en clair, même si le nom du champ ne le précise pas.
            var resultat = _hasher.VerifyHashedPassword(
                utilisateur, utilisateur.MotDePasseHash, model.MotDePasse);

            if (resultat == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new(ClaimTypes.Name, utilisateur.Nom),
                new(ClaimTypes.Email, utilisateur.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.SeSouvenir,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            // Pas de log ici : dans ce schéma, Log est lié à une Alerte
            // (AlerteId obligatoire), donc on ne peut pas tracer une simple
            // connexion. Le suivi des connexions se fera via les logs
            // serveur ASP.NET Core si besoin, pas via la table Log métier.

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Dashboard");
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET /Auth/Inscription
        [HttpGet]
        public IActionResult Inscription() => View(new InscriptionViewModel());

        // POST /Auth/Inscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscription(InscriptionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existeDeja = await _context.Utilisateurs
                .AnyAsync(u => u.Email == model.Email);
            if (existeDeja)
            {
                ModelState.AddModelError(nameof(model.Email), "Cet email est déjà utilisé.");
                return View(model);
            }

            var utilisateur = new Utilisateur
            {
                Nom = model.Nom,
                Email = model.Email
            };
            utilisateur.MotDePasseHash = _hasher.HashPassword(utilisateur, model.MotDePasse);

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            TempData["Succes"] = "Compte créé, vous pouvez vous connecter.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}