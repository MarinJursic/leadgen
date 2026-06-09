using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Leadgen.Model.Entities;

public class AppUser : IdentityUser
{
    [Required]
    [MaxLength(120)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$")]
    public string JMBG { get; set; } = string.Empty;
}
