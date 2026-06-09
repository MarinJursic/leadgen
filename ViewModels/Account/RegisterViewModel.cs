using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace leadgen.ViewModels.Account;

public class RegisterViewModel
{
    [Required]
    [MaxLength(120)]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only digits.")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only digits.")]
    public string JMBG { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public IReadOnlyList<AuthenticationScheme> ExternalLogins { get; set; } = [];
}
