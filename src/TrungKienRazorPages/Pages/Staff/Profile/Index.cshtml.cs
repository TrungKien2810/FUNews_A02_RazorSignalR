using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Repositories;
using TrungKienRazorPages.Infrastructure;
using TrungKienRazorPages.Pages.Account;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Staff.Profile;

/// <summary>Staff: view and update his/her own profile.</summary>
public class IndexModel : PageModel
{
    private readonly IAccountRepository _accountRepository;
    private readonly AdminAccountOptions _adminAccount;

    public IndexModel(IAccountRepository accountRepository, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountRepository = accountRepository;
        _adminAccount = adminAccount.Value;
    }

    public SystemAccount Account { get; set; } = null!;

    public IActionResult OnGet()
    {
        var account = _accountRepository.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        Account = account;
        return Page();
    }

    public IActionResult OnGetEdit()
    {
        var account = _accountRepository.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        return Partial("_ProfileForm", new ProfileViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? string.Empty,
            AccountEmail = account.AccountEmail ?? string.Empty
        });
    }

    public async Task<IActionResult> OnPostEditAsync(ProfileViewModel input)
    {
        // A staff can only edit his/her own profile.
        var account = _accountRepository.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        input.AccountId = account.AccountId;

        if (!string.IsNullOrEmpty(input.CurrentPassword) && input.CurrentPassword != account.AccountPassword)
        {
            ModelState.AddModelError(nameof(input.CurrentPassword), "Current password is incorrect.");
        }
        if (!string.IsNullOrWhiteSpace(input.AccountEmail))
        {
            var email = input.AccountEmail.Trim();
            if (string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
                || _accountRepository.IsEmailExisted(email, account.AccountId))
            {
                ModelState.AddModelError(nameof(input.AccountEmail), "This email is already used by another account.");
            }
        }
        if (!ModelState.IsValid)
        {
            return Partial("_ProfileForm", input);
        }

        account.AccountName = input.AccountName.Trim();
        account.AccountEmail = input.AccountEmail.Trim();
        if (!string.IsNullOrEmpty(input.NewPassword))
        {
            account.AccountPassword = input.NewPassword;
        }
        _accountRepository.UpdateAccount(account);

        // Refresh the cookie so the new name/email are shown immediately.
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            LoginModel.CreatePrincipal(account.AccountId, account.AccountName, account.AccountEmail, AccountRoles.StaffName));

        TempData["Success"] = "Your profile was updated.";
        return new JsonResult(new { success = true });
    }
}
