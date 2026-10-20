using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrungKienRazorPages.Pages;

/// <summary>
/// Where <c>UseStatusCodePagesWithReExecute</c> lands for any 4xx/5xx response that has no
/// body of its own: a mistyped address, a deleted article, a forbidden page. The page says
/// what happened in words and offers two ways back.
/// </summary>
[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class NotFoundModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Code { get; set; } = 404;

    public string Title => Code switch
    {
        403 => "That page is not yours to open",
        404 => "We could not find that page",
        410 => "That page is gone",
        >= 500 => "Something went wrong on our side",
        _ => "That request did not work"
    };

    public string Body => Code switch
    {
        403 => "Your account is signed in, but this part of the newsroom is for a different role. If you think that is wrong, ask an administrator.",
        404 => "The address may have a typo, or the article was deleted after it was linked. The newsroom board is the best place to pick the trail back up.",
        >= 500 => "The error has been logged. Try again in a moment.",
        _ => "The server declined that request. Go back, or start from the newsroom board."
    };

    public void OnGet()
    {
    }
}
