using System.Security.Claims;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Repositories;
using TrungKienRazorPages.Infrastructure;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAccountRepository _accountRepository;
    private readonly AdminAccountOptions _adminAccount;

    public LoginModel(IAccountRepository accountRepository, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountRepository = accountRepository;
        _adminAccount = adminAccount.Value;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Home");
        }
        Input.ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        short accountId;
        string name;
        string role;
        var email = Input.Email.Trim();

        if (string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
            && Input.Password == _adminAccount.Password)
        {
            accountId = 0;
            name = "Administrator";
            role = AccountRoles.AdminName;
        }
        else
        {
            SystemAccount? account;
            try
            {
                account = _accountRepository.Login(email, Input.Password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Cannot connect to the database: " + ex.GetBaseException().Message);
                return Page();
            }

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }
            if (account.AccountRole != AccountRoles.Staff && account.AccountRole != AccountRoles.Lecturer)
            {
                ModelState.AddModelError(string.Empty, "You do not have permission to access this system.");
                return Page();
            }

            accountId = account.AccountId;
            name = account.AccountName ?? account.AccountEmail ?? "User";
            role = AccountRoles.GetRoleName(account.AccountRole);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            CreatePrincipal(accountId, name, email, role));

        if (!string.IsNullOrEmpty(Input.ReturnUrl) && Url.IsLocalUrl(Input.ReturnUrl))
        {
            return LocalRedirect(Input.ReturnUrl);
        }
        return RedirectToPage("/Home");
    }

    internal static ClaimsPrincipal CreatePrincipal(short accountId, string name, string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
