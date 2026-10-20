using System.ComponentModel.DataAnnotations;

namespace TrungKienRazorPages.ViewModels;

public class AccountFormViewModel
{
    [Display(Name = "Account ID")]
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

    [Required(ErrorMessage = "Role is required.")]
    [Range(1, 2, ErrorMessage = "Role must be Staff or Lecturer.")]
    [Display(Name = "Role")]
    public int? AccountRole { get; set; }

    /// <summary>Required when creating; optional when editing (blank = keep current password).</summary>
    [StringLength(70, MinimumLength = 2, ErrorMessage = "Password must be from 2 to 70 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? AccountPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(AccountPassword), ErrorMessage = "Confirm password does not match.")]
    [Display(Name = "Confirm password")]
    public string? ConfirmPassword { get; set; }

    public bool IsEdit => AccountId > 0;
}
