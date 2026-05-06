using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Services;
using leadgen.ViewModels.ClarificationQuestions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Controllers;

// Expose clarification question list and details pages.
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

    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new ClarificationQuestionFormViewModel
        {
            MissionOptions = BuildMissionOptions()
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClarificationQuestionFormViewModel model)
    {
        ValidateAnsweredState(model);

        if (!_dbContext.BusinessDnaMissions.Any(mission => mission.Id == model.BusinessDnaMissionId))
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Selected mission was not found.");
        }

        if (!ModelState.IsValid)
        {
            model.MissionOptions = BuildMissionOptions();
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
            CreatedAtUtc = DateTime.UtcNow,
            AnsweredAtUtc = model.IsAnswered ? DateTime.UtcNow : null
        };

        _dbContext.ClarificationQuestions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.ClarificationQuestions.AsNoTracking().FirstOrDefaultAsync(question => question.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(new ClarificationQuestionFormViewModel
        {
            Id = entity.Id,
            BusinessDnaMissionId = entity.BusinessDnaMissionId,
            SlotName = entity.SlotName,
            Prompt = entity.Prompt,
            Reason = entity.Reason,
            IsAnswered = entity.IsAnswered,
            Answer = entity.Answer,
            MissionOptions = BuildMissionOptions()
        });
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ClarificationQuestionFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ValidateAnsweredState(model);

        var entity = await _dbContext.ClarificationQuestions.FirstOrDefaultAsync(question => question.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        if (!_dbContext.BusinessDnaMissions.Any(mission => mission.Id == model.BusinessDnaMissionId))
        {
            ModelState.AddModelError(nameof(model.BusinessDnaMissionId), "Selected mission was not found.");
        }

        if (!ModelState.IsValid)
        {
            model.MissionOptions = BuildMissionOptions();
            return View(model);
        }

        entity.BusinessDnaMissionId = model.BusinessDnaMissionId;
        entity.SlotName = model.SlotName.Trim();
        entity.Prompt = model.Prompt.Trim();
        entity.Reason = model.Reason.Trim();
        entity.IsAnswered = model.IsAnswered;
        entity.Answer = model.IsAnswered ? model.Answer?.Trim() : null;
        entity.AnsweredAtUtc = model.IsAnswered ? entity.AnsweredAtUtc ?? DateTime.UtcNow : null;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

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

    private IReadOnlyList<SelectListItem> BuildMissionOptions()
    {
        return _dbContext.BusinessDnaMissions
            .AsNoTracking()
            .OrderBy(mission => mission.MissionName)
            .Select(mission => new SelectListItem(mission.MissionName, mission.Id.ToString()))
            .ToList();
    }
}
