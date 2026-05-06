using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leadgen.ViewModels.ClarificationQuestions;

public sealed class ClarificationQuestionFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Mission")]
    [Required]
    public Guid BusinessDnaMissionId { get; set; }

    [Display(Name = "Slot")]
    [Required]
    [StringLength(80)]
    public string SlotName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Answered")]
    public bool IsAnswered { get; set; }

    [StringLength(500)]
    public string? Answer { get; set; }

    public IReadOnlyList<SelectListItem> MissionOptions { get; set; } = Array.Empty<SelectListItem>();
}
