using LeadGen.Infrastructure.Data;
using LeadGen.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Web.Controllers;

public sealed class ContactsController : Controller
{
    private readonly LeadGenDbContext _db;

    public ContactsController(LeadGenDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var contact = await _db.LeadContacts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (contact is null)
        {
            return NotFound();
        }

        return View(new LeadContactFormModel
        {
            Id = contact.Id,
            LeadId = contact.LeadId,
            Type = contact.Type,
            Value = contact.Value,
            SourceUrl = contact.SourceUrl,
            ConfidenceScore = contact.ConfidenceScore,
            IsVerified = contact.IsVerified
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, LeadContactFormModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var contact = await _db.LeadContacts.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (contact is null)
        {
            return NotFound();
        }

        contact.Type = model.Type;
        contact.Value = model.Value.Trim();
        contact.SourceUrl = string.IsNullOrWhiteSpace(model.SourceUrl) ? null : model.SourceUrl.Trim();
        contact.ConfidenceScore = Math.Clamp(model.ConfidenceScore, 0, 100);
        contact.IsVerified = model.IsVerified;
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Contact updated.";
        return RedirectToAction("Details", "Leads", new { id = contact.LeadId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var contact = await _db.LeadContacts.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (contact is null)
        {
            return NotFound();
        }

        var leadId = contact.LeadId;
        _db.LeadContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);
        TempData["StatusMessage"] = "Contact deleted.";
        return RedirectToAction("Details", "Leads", new { id = leadId });
    }
}
