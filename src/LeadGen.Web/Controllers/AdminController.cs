using LeadGen.Core.Configuration;
using LeadGen.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LeadGen.Web.Controllers;

public sealed class AdminController : Controller
{
    private readonly IAppLogReader _logs;
    private readonly LeadGenOptions _options;

    public AdminController(IAppLogReader logs, IOptions<LeadGenOptions> options)
    {
        _logs = logs;
        _options = options.Value;
    }

    public async Task<IActionResult> Logs(int take = 200, CancellationToken ct = default)
    {
        if (!_options.EnableAdminLogViewer)
        {
            return NotFound();
        }

        var lines = await _logs.TailAsync(take, ct);
        return View(lines);
    }
}
