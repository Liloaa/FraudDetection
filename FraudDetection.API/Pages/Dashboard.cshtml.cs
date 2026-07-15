using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FraudDetection.API.Features.Transactions;
using FraudDetection.API.Features.Alertes;
using FraudDetection.API.Features.Rapports;

namespace FraudDetection.API.Pages
    {
        [Authorize]
        public class DashboardModel : PageModel
    {
private readonly ITransactionService _transactions;
private readonly IAlerteService _alertes;
private readonly IStatistiquesService _statistiques;

        public DashboardModel(
            ITransactionService transactions,
            IAlerteService alertes,
            IStatistiquesService statistiques)
        {
_transactions = transactions;
_alertes = alertes;
_statistiques = statistiques;
    }

public ResumeStatistiques Resume { get; set; } = new(0, 0, 0, 0, 0, 0);
public List<Transaction>
DernieresTransactions { get; set; } = new();
public List<Alerte>
AlertesRecentes { get; set; } = new();

public async Task OnGetAsync()
{
Resume = await _statistiques.GetResumeAsync();
DernieresTransactions = (await _transactions.GetAllAsync()).Take(8).ToList();
AlertesRecentes = (await _alertes.GetEnAttenteAsync()).Take(6).ToList();
}
}
}