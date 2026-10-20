using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrungKienRazorPages.ViewModels;

public class CategoryFormViewModel
{
    [Display(Name = "Category ID")]
    public short CategoryId { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be from 2 to 100 characters.")]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(250, MinimumLength = 2, ErrorMessage = "Description must be from 2 to 250 characters.")]
    [Display(Name = "Description")]
    public string CategoryDesciption { get; set; } = string.Empty;

    [Display(Name = "Parent category")]
    public short? ParentCategoryId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [BindNever, ValidateNever]
    public List<SelectListItem> ParentOptions { get; set; } = new();

    public bool IsEdit => CategoryId > 0;
}
