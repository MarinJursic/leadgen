using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class ContactChannel
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(TargetContact))]
    public Guid TargetContactId { get; set; }

    public ContactChannelType Type { get; set; }

    [Required]
    [MaxLength(320)]
    public string Value { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    [Required]
    [MaxLength(160)]
    public string Source { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public virtual TargetContact? TargetContact { get; set; }
}
