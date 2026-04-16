using leadgen.Services;
using leadgen.ViewModels.ClarificationQuestions;
using Microsoft.AspNetCore.Mvc;

namespace leadgen.Controllers;

public sealed class ClarificationQuestionsController : Controller
{
    private readonly ILeadgenReadRepository _repository;

    public ClarificationQuestionsController(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var questions = _repository.GetClarificationQuestions()
            .OrderByDescending(question => question.CreatedAtUtc)
            .ToList();

        return View(questions);
    }

    public IActionResult Details(Guid id)
    {
        var question = _repository.GetClarificationQuestion(id);
        if (question is null)
        {
            return NotFound();
        }

        var mission = _repository.GetMissions()
            .FirstOrDefault(item => item.ClarificationQuestions.Any(candidate => candidate.Id == id));

        return View(new ClarificationQuestionDetailsViewModel
        {
            Question = question,
            Mission = mission
        });
    }
}
