using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FraudDetection.API.Features.Rapports;

namespace FraudDetection.API.Pages
{
[Authorize]
public class RapportsModel : PageModel
{
private readonly IStatistiquesService _statistiques;

public RapportsModel(IStatistiquesService statistiques)
{
_statistiques = statistiques;
}

public ResumeStatistiques Resume { get; set; } = new(0, 0, 0, 0, 0, 0);

public async Task OnGetAsync()
{
// Chargement initial côté serveur ; les graphiques détaillés
// (par jour / par lieu) sont récupérés en JS via /api/rapports/...
Resume = await _statistiques.GetResumeAsync();
}
}
}