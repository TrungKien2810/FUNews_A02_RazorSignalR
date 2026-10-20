using BusinessObjects.Models;

namespace TrungKienRazorPages.ViewModels;

public class DeleteCategoryViewModel
{
    public Category Category { get; set; } = null!;
    public bool CanDelete { get; set; }
    public int NewsCount { get; set; }
}
