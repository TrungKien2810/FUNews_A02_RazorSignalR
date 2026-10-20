using BusinessObjects.Models;

namespace Repositories;

public interface IAccountRepository
{
    List<SystemAccount> GetAccounts();
    List<SystemAccount> SearchAccounts(string? keyword, int? role);
    SystemAccount? GetAccountById(short id);
    SystemAccount? GetAccountByEmail(string email);
    SystemAccount? Login(string email, string password);
    bool IsEmailExisted(string email, short? excludeAccountId = null);
    bool HasNewsArticles(short accountId);
    void AddAccount(SystemAccount account);
    void UpdateAccount(SystemAccount account);
    void DeleteAccount(short id);
}
