using System.ComponentModel.DataAnnotations;

namespace leadgen.ViewModels.Account;

public class ExternalLoginConfirmationViewModel
{
    public string Provider { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only digits.")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only digits.")]
    public string JMBG { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
