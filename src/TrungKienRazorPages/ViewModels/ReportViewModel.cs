using System.ComponentModel.DataAnnotations;

namespace TrungKienRazorPages.ViewModels;

public class ReportViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime? EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
        {
            yield return new ValidationResult("Start date must be before or equal to end date.", new[] { nameof(EndDate) });
        }
    }
}
