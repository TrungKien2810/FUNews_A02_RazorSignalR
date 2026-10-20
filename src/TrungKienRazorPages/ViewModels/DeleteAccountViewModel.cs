using BusinessObjects.Models;

namespace TrungKienRazorPages.ViewModels;

public class DeleteAccountViewModel
{
    public SystemAccount Account { get; set; } = null!;
    public bool CanDelete { get; set; }
}
