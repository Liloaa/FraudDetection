using FraudDetection.API.Features.Transactions;
using FraudDetection.API.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Base de données SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + Pages Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SignalR (Membre 3 en aura besoin)
builder.Services.AddSignalR();

builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();