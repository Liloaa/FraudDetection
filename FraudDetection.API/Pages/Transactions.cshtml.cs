using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FraudDetection.API.Features.Transactions;

namespace FraudDetection.API.Pages
{
[Authorize]
public class TransactionsModel : PageModel
{
private readonly ITransactionService _transactions;

public TransactionsModel(ITransactionService transactions)
{
_transactions = transactions;
}

[BindProperty(SupportsGet = true)]
public string? Statut { get; set; }

[BindProperty(SupportsGet = true)]
public DateTime? DateDebut { get; set; }

[BindProperty(SupportsGet = true)]
public DateTime? DateFin { get; set; }

public List<Transaction>
Transactions { get; set; } = new();
public string? Message { get; set; }

public async Task OnGetAsync()
{
Transactions = await _transactions.FiltrerAsync(Statut, DateDebut, DateFin);
}

// Import CSV : CompteId,Montant,Lieu,Type,DateTransaction
public async Task<IActionResult>
OnPostImporterAsync(IFormFile? fichier)
{
if (fichier == null || fichier.Length == 0)
{
Message = "Choisissez un fichier CSV.";
}
else
{
await _transactions.ImporterCsvAsync(fichier);
Message = "Import terminé.";
}

Transactions = await _transactions.FiltrerAsync(Statut, DateDebut, DateFin);
return Page();
}
}
}