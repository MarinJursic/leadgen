using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.ClarificationQuestions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose clarification question list and details pages.
[Authorize]
[Route("questions")]
public sealed class ClarificationQuestionsController : Controller
{
    // Read-only access to the seeded Leadgen dataset.
    private readonly ILeadgenReadRepository _repository;
    private readonly LeadgenDbContext _dbContext;

    // Receive the repository from dependency injection.
    public ClarificationQuestionsController(ILeadgenReadRepository repository, LeadgenDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    // Show all clarification questions ordered by newest first.
    [AllowAnonymous]
    [HttpGet("")]
    public IActionResult Index()
    {
        // Flattened questions are already provided by the repository.
        var questions = _repository.GetClarificationQuestions()
            .OrderByDescending(question => question.CreatedAtUtc)
            .ToList();

        return View(questions);
    }

    // Show one clarification question and the mission that owns it.
    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        // Find the requested question by id.
        var question = _repository.GetClarificationQuestion(id);
        if (question is null)
        {
            return NotFound();
        }

        // Resolve the parent mission by scanning the object graph.
        var mission = _repository.GetMissions()
            .FirstOrDefault(item => item.ClarificationQuestions.Any(candidate => candidate.Id == id));

        // Send both the question and its parent mission to the view.
        return View(new ClarificationQuestionDetailsViewModel
        {
            Question = question,
            Mission = mission
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new ClarificationQuestionFormViewModel
        {
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClarificationQuestionFormViewModel model)
    {
        ValidateAnsweredState(model);

        ValidateQuestionDates(model);

        if (!await _dbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == model.BusinessDnaMissionId))
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Selected mission was not found.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = new ClarificationQuestion
        {
            Id = Guid.NewGuid(),
            BusinessDnaMissionId = model.BusinessDnaMissionId,
            SlotName = model.SlotName.Trim(),
            Prompt = model.Prompt.Trim(),
            Reason = model.Reason.Trim(),
            IsAnswered = model.IsAnswered,
            Answer = model.IsAnswered ? model.Answer?.Trim() : null,
            CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc),
            AnsweredAtUtc = model.IsAnswered ? NormalizeUtc(model.AnsweredAtUtc ?? DateTime.UtcNow) : null
        };

        _dbContext.ClarificationQuestions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.ClarificationQuestions.AsNoTracking()
            .Include(question => question.Mission)
            .FirstOrDefaultAsync(question => question.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(new ClarificationQuestionFormViewModel
        {
            Id = entity.Id,
            BusinessDnaMissionId = entity.BusinessDnaMissionId,
            BusinessDnaMissionName = entity.Mission?.MissionName,
            SlotName = entity.SlotName,
            Prompt = entity.Prompt,
            Reason = entity.Reason,
            IsAnswered = entity.IsAnswered,
            Answer = entity.Answer,
            CreatedAtUtc = entity.CreatedAtUtc,
            AnsweredAtUtc = entity.AnsweredAtUtc
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ClarificationQuestionFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ValidateAnsweredState(model);
        ValidateQuestionDates(model);

        var entity = await _dbContext.ClarificationQuestions.FirstOrDefaultAsync(question => question.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        if (!await _dbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == model.BusinessDnaMissionId))
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Selected mission was not found.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        entity.BusinessDnaMissionId = model.BusinessDnaMissionId;
        entity.SlotName = model.SlotName.Trim();
        entity.Prompt = model.Prompt.Trim();
        entity.Reason = model.Reason.Trim();
        entity.IsAnswered = model.IsAnswered;
        entity.Answer = model.IsAnswered ? model.Answer?.Trim() : null;
        entity.CreatedAtUtc = NormalizeUtc(model.CreatedAtUtc);
        entity.AnsweredAtUtc = model.IsAnswered ? NormalizeUtc(model.AnsweredAtUtc ?? entity.AnsweredAtUtc ?? DateTime.UtcNow) : null;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var model = await BuildDeleteViewModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, ClarificationQuestionDeleteViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var entity = await _dbContext.ClarificationQuestions.FirstOrDefaultAsync(question => question.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.ClarificationQuestions.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void ValidateAnsweredState(ClarificationQuestionFormViewModel model)
    {
        if (model.IsAnswered && string.IsNullOrWhiteSpace(model.Answer))
        {
            ModelState.AddModelError(nameof(model.Answer), "Answered questions must include an answer.");
        }
    }

    private void ValidateQuestionDates(ClarificationQuestionFormViewModel model)
    {
        if (model.BusinessDnaMissionId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Select a mission.");
        }

        if (model.IsAnswered && model.AnsweredAtUtc.HasValue && model.AnsweredAtUtc.Value < model.CreatedAtUtc)
        {
            ModelState.AddModelError(nameof(model.AnsweredAtUtc), "Answered time cannot be before the question was created.");
        }
    }

    private async Task<ClarificationQuestionDeleteViewModel?> BuildDeleteViewModelAsync(Guid id)
    {
        return await _dbContext.ClarificationQuestions
            .AsNoTracking()
            .Include(question => question.Mission)
            .Where(question => question.Id == id)
            .Select(question => new ClarificationQuestionDeleteViewModel
            {
                Id = question.Id,
                SlotName = question.SlotName,
                Prompt = question.Prompt,
                Reason = question.Reason,
                IsAnswered = question.IsAnswered,
                Answer = question.Answer,
                MissionName = question.Mission != null ? question.Mission.MissionName : "Unknown",
                CreatedAtUtc = question.CreatedAtUtc,
                AnsweredAtUtc = question.AnsweredAtUtc
            })
            .FirstOrDefaultAsync();
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
