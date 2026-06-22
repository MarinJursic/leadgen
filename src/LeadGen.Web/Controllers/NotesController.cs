using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class NotesController : Controller
{
    private readonly LeadGenDbContext _db;

    public NotesController(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var note = await _db.LeadNotes.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (note is null)
        {
            return NotFound();
        }

        return View(new LeadNoteFormModel
        {
            Id = note.Id,
            LeadId = note.LeadId,
            Body = note.Body
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, LeadNoteFormModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var note = await _db.LeadNotes.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (note is null)
        {
            return NotFound();
        }

        note.Body = model.Body.Trim();
        note.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Note updated.";
        return RedirectToAction("Details", "Leads", new { id = note.LeadId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var note = await _db.LeadNotes.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (note is null)
        {
            return NotFound();
        }

        var leadId = note.LeadId;
        _db.LeadNotes.Remove(note);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Note deleted.";
        return RedirectToAction("Details", "Leads", new { id = leadId });
    }
}
