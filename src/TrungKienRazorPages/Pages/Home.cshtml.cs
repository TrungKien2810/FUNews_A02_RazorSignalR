using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrungKienRazorPages.Pages;

/// <summary>Sends the user to the start page of his/her role.</summary>
public class HomeModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.IsInRole(AccountRoles.AdminName)) return RedirectToPage("/Admin/Accounts/Index");
        if (User.IsInRole(AccountRoles.StaffName)) return RedirectToPage("/Staff/News/Index");
        if (User.IsInRole(AccountRoles.LecturerName)) return RedirectToPage("/News/Index");
        return RedirectToPage("/Account/Login");
    }
}
