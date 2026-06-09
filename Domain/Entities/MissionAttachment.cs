using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class MissionAttachment
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Mission))]
    public Guid BusinessDnaMissionId { get; set; }

    [Required]
    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(260)]
    public string StorageFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [MaxLength(450)]
    public string? UploadedByUserId { get; set; }

    public virtual BusinessDnaMission? Mission { get; set; }
}
