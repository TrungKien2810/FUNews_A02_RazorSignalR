using System.ComponentModel.DataAnnotations;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TrungKienRazorPages.ViewModels;

public class NewsFormViewModel
{
    [Display(Name = "News ID")]
    public string? NewsArticleId { get; set; }

    [Required(ErrorMessage = "News title is required.")]
    [StringLength(400, MinimumLength = 5, ErrorMessage = "News title must be from 5 to 400 characters.")]
    [Display(Name = "Title")]
    public string NewsTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Headline is required.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Headline must be from 5 to 150 characters.")]
    [Display(Name = "Headline")]
    public string Headline { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Content must be from 10 to 4000 characters.")]
    [Display(Name = "Content")]
    public string NewsContent { get; set; } = string.Empty;

    [StringLength(400, ErrorMessage = "Source must be at most 400 characters.")]
    [Display(Name = "Source")]
    public string? NewsSource { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public short? CategoryId { get; set; }

    [Display(Name = "Active")]
    public bool NewsStatus { get; set; } = true;

    [Display(Name = "Tags")]
    public List<int> SelectedTagIds { get; set; } = new();

    // Options for the dropdown / checkboxes (filled by the page, never posted).
    [BindNever, ValidateNever]
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    [BindNever, ValidateNever]
    public List<Tag> TagOptions { get; set; } = new();

    public bool IsEdit => !string.IsNullOrEmpty(NewsArticleId);
}
