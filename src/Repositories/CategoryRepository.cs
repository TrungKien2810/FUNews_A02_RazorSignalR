using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class CategoryRepository : ICategoryRepository
{
    public List<Category> GetCategories() => CategoryDAO.Instance.GetCategories();
    public List<Category> GetActiveCategories() => CategoryDAO.Instance.GetActiveCategories();
    public List<Category> SearchCategories(string? keyword, bool? isActive) => CategoryDAO.Instance.SearchCategories(keyword, isActive);
    public Category? GetCategoryById(short id) => CategoryDAO.Instance.GetCategoryById(id);
    public bool IsCategoryInUse(short id) => CategoryDAO.Instance.IsCategoryInUse(id);
    public int CountNewsArticles(short id) => CategoryDAO.Instance.CountNewsArticles(id);
    public void AddCategory(Category category) => CategoryDAO.Instance.AddCategory(category);
    public void UpdateCategory(Category category) => CategoryDAO.Instance.UpdateCategory(category);
    public void DeleteCategory(short id) => CategoryDAO.Instance.DeleteCategory(id);
}
