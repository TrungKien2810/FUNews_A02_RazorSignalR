using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Repositories;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Staff.Categories;

/// <summary>Staff: manage categories (CRUD + search). A category used by news articles cannot be deleted.</summary>
public class IndexModel : PageModel
{
    private readonly ICategoryRepository _categoryRepository;

    public IndexModel(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }

    public List<Category> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = _categoryRepository.SearchCategories(Keyword, IsActive);
    }

    public IActionResult OnGetCreate() => FormPartial(new CategoryFormViewModel());

    public IActionResult OnPostCreate(CategoryFormViewModel input)
    {
        ValidateParent(input);
        if (!ModelState.IsValid)
        {
            return FormPartial(input);
        }

        _categoryRepository.AddCategory(new Category
        {
            CategoryName = input.CategoryName.Trim(),
            CategoryDesciption = input.CategoryDesciption.Trim(),
            ParentCategoryId = input.ParentCategoryId,
            IsActive = input.IsActive
        });
        TempData["Success"] = $"Category \"{input.CategoryName}\" was created.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetEdit(short id)
    {
        var category = _categoryRepository.GetCategoryById(id);
        if (category == null) return NotFound();
        return FormPartial(new CategoryFormViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryDesciption = category.CategoryDesciption,
            // In the sample data a root category points to itself; show it as "no parent".
            ParentCategoryId = category.ParentCategoryId == category.CategoryId ? null : category.ParentCategoryId,
            IsActive = category.IsActive == true
        });
    }

    public IActionResult OnPostEdit(CategoryFormViewModel input)
    {
        var category = _categoryRepository.GetCategoryById(input.CategoryId);
        if (category == null) return NotFound();

        ValidateParent(input);
        if (!ModelState.IsValid)
        {
            return FormPartial(input);
        }

        category.CategoryName = input.CategoryName.Trim();
        category.CategoryDesciption = input.CategoryDesciption.Trim();
        category.ParentCategoryId = input.ParentCategoryId;
        category.IsActive = input.IsActive;
        _categoryRepository.UpdateCategory(category);
        TempData["Success"] = $"Category \"{input.CategoryName}\" was updated.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetDelete(short id)
    {
        var category = _categoryRepository.GetCategoryById(id);
        if (category == null) return NotFound();
        return Partial("_DeleteCategory", new DeleteCategoryViewModel
        {
            Category = category,
            CanDelete = !_categoryRepository.IsCategoryInUse(id),
            NewsCount = _categoryRepository.CountNewsArticles(id)
        });
    }

    public IActionResult OnPostDelete(short id)
    {
        var category = _categoryRepository.GetCategoryById(id);
        if (category == null) return NotFound();
        if (_categoryRepository.IsCategoryInUse(id))
        {
            return new JsonResult(new { success = false, message = "This category is already used by news articles and cannot be deleted." });
        }
        _categoryRepository.DeleteCategory(id);
        TempData["Success"] = $"Category \"{category.CategoryName}\" was deleted.";
        return new JsonResult(new { success = true });
    }

    private void ValidateParent(CategoryFormViewModel input)
    {
        if (input.ParentCategoryId.HasValue && _categoryRepository.GetCategoryById(input.ParentCategoryId.Value) == null)
        {
            ModelState.AddModelError(nameof(input.ParentCategoryId), "Parent category does not exist.");
        }
        if (input.IsEdit && input.ParentCategoryId == input.CategoryId)
        {
            ModelState.AddModelError(nameof(input.ParentCategoryId), "A category cannot be its own parent.");
        }
    }

    private PartialViewResult FormPartial(CategoryFormViewModel model)
    {
        model.ParentOptions = _categoryRepository.GetCategories()
            .Where(c => !model.IsEdit || c.CategoryId != model.CategoryId)
            .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString()))
            .ToList();
        return Partial("_CategoryForm", model);
    }
}
