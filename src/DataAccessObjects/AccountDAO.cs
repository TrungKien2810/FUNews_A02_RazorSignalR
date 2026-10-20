using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class AccountDAO
{
    private static AccountDAO? _instance;
    private static readonly object InstanceLock = new();

    private AccountDAO() { }

    public static AccountDAO Instance
    {
        get
        {
            lock (InstanceLock)
            {
                return _instance ??= new AccountDAO();
            }
        }
    }

    public List<SystemAccount> GetAccounts()
    {
        using var db = new FUNewsManagementContext();
        return db.SystemAccounts.AsNoTracking().OrderBy(a => a.AccountId).ToList();
    }

    public List<SystemAccount> SearchAccounts(string? keyword, int? role)
    {
        using var db = new FUNewsManagementContext();
        var query = db.SystemAccounts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            query = query.Where(a => (a.AccountName != null && a.AccountName.Contains(key))
                                     || (a.AccountEmail != null && a.AccountEmail.Contains(key)));
        }
        if (role.HasValue)
        {
            query = query.Where(a => a.AccountRole == role.Value);
        }
        return query.OrderBy(a => a.AccountId).ToList();
    }

    public SystemAccount? GetAccountById(short id)
    {
        using var db = new FUNewsManagementContext();
        return db.SystemAccounts.AsNoTracking().FirstOrDefault(a => a.AccountId == id);
    }

    public SystemAccount? GetAccountByEmail(string email)
    {
        using var db = new FUNewsManagementContext();
        return db.SystemAccounts.AsNoTracking().FirstOrDefault(a => a.AccountEmail == email);
    }

    public SystemAccount? Login(string email, string password)
    {
        using var db = new FUNewsManagementContext();
        var account = db.SystemAccounts.AsNoTracking().FirstOrDefault(a => a.AccountEmail == email);
        // Compare password in memory so the comparison is case-sensitive regardless of DB collation.
        return account != null && account.AccountPassword == password ? account : null;
    }

    public bool IsEmailExisted(string email, short? excludeAccountId = null)
    {
        using var db = new FUNewsManagementContext();
        return db.SystemAccounts.Any(a => a.AccountEmail == email
                                          && (!excludeAccountId.HasValue || a.AccountId != excludeAccountId.Value));
    }

    public bool HasNewsArticles(short accountId)
    {
        using var db = new FUNewsManagementContext();
        return db.NewsArticles.Any(n => n.CreatedById == accountId);
    }

    public void AddAccount(SystemAccount account)
    {
        using var db = new FUNewsManagementContext();
        // AccountID is not an identity column, so generate the next id.
        account.AccountId = (short)((db.SystemAccounts.Max(a => (short?)a.AccountId) ?? 0) + 1);
        db.SystemAccounts.Add(account);
        db.SaveChanges();
    }

    public void UpdateAccount(SystemAccount account)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.SystemAccounts.FirstOrDefault(a => a.AccountId == account.AccountId)
                       ?? throw new InvalidOperationException("Account not found.");
        existing.AccountName = account.AccountName;
        existing.AccountEmail = account.AccountEmail;
        existing.AccountRole = account.AccountRole;
        existing.AccountPassword = account.AccountPassword;
        db.SaveChanges();
    }

    public void DeleteAccount(short id)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.SystemAccounts.FirstOrDefault(a => a.AccountId == id)
                       ?? throw new InvalidOperationException("Account not found.");
        db.SystemAccounts.Remove(existing);
        db.SaveChanges();
    }
}
