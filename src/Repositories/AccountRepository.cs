using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class AccountRepository : IAccountRepository
{
    public List<SystemAccount> GetAccounts() => AccountDAO.Instance.GetAccounts();
    public List<SystemAccount> SearchAccounts(string? keyword, int? role) => AccountDAO.Instance.SearchAccounts(keyword, role);
    public SystemAccount? GetAccountById(short id) => AccountDAO.Instance.GetAccountById(id);
    public SystemAccount? GetAccountByEmail(string email) => AccountDAO.Instance.GetAccountByEmail(email);
    public SystemAccount? Login(string email, string password) => AccountDAO.Instance.Login(email, password);
    public bool IsEmailExisted(string email, short? excludeAccountId = null) => AccountDAO.Instance.IsEmailExisted(email, excludeAccountId);
    public bool HasNewsArticles(short accountId) => AccountDAO.Instance.HasNewsArticles(accountId);
    public void AddAccount(SystemAccount account) => AccountDAO.Instance.AddAccount(account);
    public void UpdateAccount(SystemAccount account) => AccountDAO.Instance.UpdateAccount(account);
    public void DeleteAccount(short id) => AccountDAO.Instance.DeleteAccount(id);
}
