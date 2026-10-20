namespace BusinessObjects.Models;

/// <summary>
/// Account roles. Staff and Lecturer are stored in the database (SystemAccount.AccountRole);
/// Admin is not stored in the database, its credentials come from appsettings.json.
/// </summary>
public static class AccountRoles
{
    public const int Staff = 1;
    public const int Lecturer = 2;

    public const string AdminName = "Admin";
    public const string StaffName = "Staff";
    public const string LecturerName = "Lecturer";

    public static string GetRoleName(int? role) => role switch
    {
        Staff => StaffName,
        Lecturer => LecturerName,
        _ => "Unknown"
    };
}
