using System.ComponentModel.DataAnnotations;

namespace TrungKienRazorPages.ViewModels;

public class ProfileViewModel : IValidatableObject
{
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Account name must be from 2 to 100 characters.")]
    [Display(Name = "Account name")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    [StringLength(70, ErrorMessage = "Email must be at most 70 characters.")]
    [Display(Name = "Email")]
    public string AccountEmail { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string? CurrentPassword { get; set; }

    [StringLength(70, MinimumLength = 2, ErrorMessage = "New password must be from 2 to 70 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password (leave blank to keep)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Confirm password does not match.")]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // The form tells the staff member that leaving the password fields blank keeps
        // the current password, so a name/email-only save must not demand it. Setting a
        // new one still has to prove the old one.
        if (!string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(CurrentPassword))
        {
            yield return new ValidationResult("Current password is required to set a new password.",
                new[] { nameof(CurrentPassword) });
        }
    }
}
