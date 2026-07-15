using FraudDetection.API.Features.Alertes;
using FraudDetection.API.Features.Auth;
using FraudDetection.API.Features.Rapports;
using FraudDetection.API.Features.Transactions;
using FraudDetection.API.Services;
using FraudDetection.API.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

builder.Services.AddSignalR();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<Utilisateur>, PasswordHasher<Utilisateur>>();

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAlerteService, AlerteService>();
builder.Services.AddScoped<IStatistiquesService, StatistiquesService>();
builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Clients.Any())
    {
        var client1 = new Client
        {
            Nom = "Rakoto",
            Prenom = "Jean",
            Email = "jean.rakoto@mail.com",
            Telephone = "+261 34 00 000 01"
        };
        var client2 = new Client
        {
            Nom = "Andry",
            Prenom = "Marie",
            Email = "marie.andry@mail.com",
            Telephone = "+261 34 00 000 02"
        };
        context.Clients.AddRange(client1, client2);
        context.SaveChanges();

        var compte1 = new Compte
        {
            ClientId = client1.Id,
            NumeroCompte = "MG46-0001-1111",
            Type = "Courant",
            SoldeMoyen = 2300,
            LieuHabituel = "Antananarivo"
        };
        var compte2 = new Compte
        {
            ClientId = client2.Id,
            NumeroCompte = "MG46-0002-2222",
            Type = "Epargne",
            SoldeMoyen = 5000,
            LieuHabituel = "Antananarivo"
        };
        context.Comptes.AddRange(compte1, compte2);
        context.SaveChanges();

        var transactionsSeed = new List<Transaction>
        {
            new Transaction
            {
                CompteId = compte1.Id,
                Montant = 150,
                Lieu = "Antananarivo",
                Type = "Retrait",
                Statut = "Normal",
                DateTransaction = DateTime.UtcNow.AddDays(-3)
            },
            new Transaction
            {
                CompteId = compte1.Id,
                Montant = 8500,
                Lieu = "Singapour",
                Type = "Virement",
                Statut = "Normal",
                DateTransaction = DateTime.UtcNow.AddDays(-1)
            },
            new Transaction
            {
                CompteId = compte2.Id,
                Montant = 300,
                Lieu = "Antananarivo",
                Type = "Paiement",
                Statut = "Normal",
                DateTransaction = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Transactions.AddRange(transactionsSeed);
        context.SaveChanges();

        var fraudService = scope.ServiceProvider.GetRequiredService<IFraudDetectionService>();
        var alerteService = scope.ServiceProvider.GetRequiredService<IAlerteService>();

        foreach (var t in transactionsSeed)
        {
            var (score, raison) = fraudService.AnalyserAsync(t).GetAwaiter().GetResult();

            t.Statut = score >= 0.5f ? "Suspect" : "Normal";

            if (score >= 0.5f)
            {
                alerteService.CreerDepuisTransactionAsync(t.Id, score, raison)
                    .GetAwaiter().GetResult();
            }
        }

        context.SaveChanges();
    }

    if (!context.Utilisateurs.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Utilisateur>>();
        var compteTest = new Utilisateur
        {
            Nom = "Admin",
            Email = "admin@frauddetection.mg"
        };
        compteTest.MotDePasseHash = hasher.HashPassword(compteTest, "Admin123!");
        context.Utilisateurs.Add(compteTest);
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");
app.MapRazorPages();
app.MapHub<AlerteHub>("/hubs/alertes");

app.Run();