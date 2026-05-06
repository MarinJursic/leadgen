using System.ComponentModel.DataAnnotations;

namespace leadgen.ViewModels.ClarificationQuestions;

public sealed class ClarificationQuestionDeleteViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string SlotName { get; init; } = string.Empty;

    public string Prompt { get; init; } = string.Empty;

    public string Reason { get; init; } = string.Empty;

    public bool IsAnswered { get; init; }

    public string? Answer { get; init; }

    public string MissionName { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? AnsweredAtUtc { get; init; }
}
