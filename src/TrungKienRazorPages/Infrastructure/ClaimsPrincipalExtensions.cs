using System.Security.Claims;

namespace TrungKienRazorPages.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    /// <summary>AccountID of the signed in user (0 for the admin account, which is not stored in the database).</summary>
    public static short GetAccountId(this ClaimsPrincipal user)
        => short.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (short)0;
}
