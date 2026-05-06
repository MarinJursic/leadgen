using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class ClarificationQuestion
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Mission))]
    public Guid BusinessDnaMissionId { get; set; }

    [Required]
    [MaxLength(80)]
    public string SlotName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public bool IsAnswered { get; set; }

    [MaxLength(500)]
    public string? Answer { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? AnsweredAtUtc { get; set; }

    public virtual BusinessDnaMission? Mission { get; set; }
}
