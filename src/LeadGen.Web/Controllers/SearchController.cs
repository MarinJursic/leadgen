using LeadGen.Core.Services;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeadGen.Web.Controllers;

public sealed class SearchController : Controller
{
    private readonly IGlobalSearchService _search;

    public SearchController(IGlobalSearchService search)
    {
        _search = search;
    }

    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        var results = await _search.SearchAsync(q, 75, ct);
        return View(new SearchPageViewModel(q, results));
    }
}
