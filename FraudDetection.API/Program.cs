using FraudDetection.API.Features.Transactions;
using FraudDetection.API.Features.Alertes;
using FraudDetection.API.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ← CORRECTION ICI
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAlerteService, AlerteService>();
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

        context.Transactions.AddRange(
            new FraudDetection.API.Features.Transactions.Transaction
            {
                CompteId = compte1.Id,
                Montant = 150,
                Lieu = "Antananarivo",
                Type = "Retrait",
                Statut = "Normal",
                DateTransaction = DateTime.UtcNow.AddDays(-3)
            },
            new FraudDetection.API.Features.Transactions.Transaction
            {
                CompteId = compte1.Id,
                Montant = 8500,
                Lieu = "Singapour",
                Type = "Virement",
                Statut = "Suspect",
                DateTransaction = DateTime.UtcNow.AddDays(-1)
            },
            new FraudDetection.API.Features.Transactions.Transaction
            {
                CompteId = compte2.Id,
                Montant = 300,
                Lieu = "Antananarivo",
                Type = "Paiement",
                Statut = "Normal",
                DateTransaction = DateTime.UtcNow.AddDays(-2)
            }
        );
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<AlerteHub>("/hubs/alertes");

app.Run();