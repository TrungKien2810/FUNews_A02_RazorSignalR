using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Repositories;
using TrungKienRazorPages.Infrastructure;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Admin.Accounts;

/// <summary>Admin: manage system accounts (CRUD + search). Create/Edit/Delete run in popup dialogs.</summary>
public class IndexModel : PageModel
{
    private readonly IAccountRepository _accountRepository;
    private readonly AdminAccountOptions _adminAccount;

    public IndexModel(IAccountRepository accountRepository, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountRepository = accountRepository;
        _adminAccount = adminAccount.Value;
    }

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public int? Role { get; set; }

    public List<SystemAccount> Accounts { get; set; } = new();

    public void OnGet()
    {
        Accounts = _accountRepository.SearchAccounts(Keyword, Role);
    }

    public IActionResult OnGetCreate() => Partial("_AccountForm", new AccountFormViewModel { AccountRole = AccountRoles.Staff });

    public IActionResult OnPostCreate(AccountFormViewModel input)
    {
        if (string.IsNullOrEmpty(input.AccountPassword))
        {
            ModelState.AddModelError(nameof(input.AccountPassword), "Password is required.");
        }
        ValidateEmail(input);
        if (!ModelState.IsValid)
        {
            return Partial("_AccountForm", input);
        }

        _accountRepository.AddAccount(new SystemAccount
        {
            AccountName = input.AccountName.Trim(),
            AccountEmail = input.AccountEmail.Trim(),
            AccountRole = input.AccountRole,
            AccountPassword = input.AccountPassword
        });
        TempData["Success"] = $"Account \"{input.AccountName}\" was created.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetEdit(short id)
    {
        var account = _accountRepository.GetAccountById(id);
        if (account == null) return NotFound();
        return Partial("_AccountForm", new AccountFormViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? string.Empty,
            AccountEmail = account.AccountEmail ?? string.Empty,
            AccountRole = account.AccountRole
        });
    }

    public IActionResult OnPostEdit(AccountFormViewModel input)
    {
        var account = _accountRepository.GetAccountById(input.AccountId);
        if (account == null) return NotFound();

        ValidateEmail(input);
        if (!ModelState.IsValid)
        {
            return Partial("_AccountForm", input);
        }

        account.AccountName = input.AccountName.Trim();
        account.AccountEmail = input.AccountEmail.Trim();
        account.AccountRole = input.AccountRole;
        if (!string.IsNullOrEmpty(input.AccountPassword))
        {
            account.AccountPassword = input.AccountPassword;
        }
        _accountRepository.UpdateAccount(account);
        TempData["Success"] = $"Account \"{input.AccountName}\" was updated.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetDelete(short id)
    {
        var account = _accountRepository.GetAccountById(id);
        if (account == null) return NotFound();
        return Partial("_DeleteAccount", new DeleteAccountViewModel
        {
            Account = account,
            CanDelete = !_accountRepository.HasNewsArticles(id)
        });
    }

    public IActionResult OnPostDelete(short id)
    {
        var account = _accountRepository.GetAccountById(id);
        if (account == null) return NotFound();
        // An account that already created news articles cannot be deleted.
        if (_accountRepository.HasNewsArticles(id))
        {
            return new JsonResult(new { success = false, message = "This account has created news articles and cannot be deleted." });
        }
        _accountRepository.DeleteAccount(id);
        TempData["Success"] = $"Account \"{account.AccountName}\" was deleted.";
        return new JsonResult(new { success = true });
    }

    private void ValidateEmail(AccountFormViewModel input)
    {
        if (string.IsNullOrWhiteSpace(input.AccountEmail)) return;
        var email = input.AccountEmail.Trim();
        if (string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
            || _accountRepository.IsEmailExisted(email, input.IsEdit ? input.AccountId : null))
        {
            ModelState.AddModelError(nameof(input.AccountEmail), "This email is already used by another account.");
        }
    }
}
