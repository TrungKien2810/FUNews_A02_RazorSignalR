namespace TrungKienRazorPages.Infrastructure;

/// <summary>Default admin account, bound from the "AdminAccount" section of appsettings.json.</summary>
public class AdminAccountOptions
{
    public const string SectionName = "AdminAccount";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
