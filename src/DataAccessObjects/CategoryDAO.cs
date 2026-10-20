using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class CategoryDAO
{
    private static CategoryDAO? _instance;
    private static readonly object InstanceLock = new();

    private CategoryDAO() { }

    public static CategoryDAO Instance
    {
        get
        {
            lock (InstanceLock)
            {
                return _instance ??= new CategoryDAO();
            }
        }
    }

    public List<Category> GetCategories()
    {
        using var db = new FUNewsManagementContext();
        return db.Categories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.CategoryId)
            .ToList();
    }

    public List<Category> GetActiveCategories()
    {
        using var db = new FUNewsManagementContext();
        return db.Categories.AsNoTracking()
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.CategoryName)
            .ToList();
    }

    public List<Category> SearchCategories(string? keyword, bool? isActive)
    {
        using var db = new FUNewsManagementContext();
        var query = db.Categories.AsNoTracking().Include(c => c.ParentCategory).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            query = query.Where(c => c.CategoryName.Contains(key) || c.CategoryDesciption.Contains(key));
        }
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }
        return query.OrderBy(c => c.CategoryId).ToList();
    }

    public Category? GetCategoryById(short id)
    {
        using var db = new FUNewsManagementContext();
        return db.Categories.AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefault(c => c.CategoryId == id);
    }

    /// <summary>A category is in use when a news article belongs to it or another category uses it as parent.</summary>
    public bool IsCategoryInUse(short id)
    {
        using var db = new FUNewsManagementContext();
        return db.NewsArticles.Any(n => n.CategoryId == id)
               || db.Categories.Any(c => c.ParentCategoryId == id && c.CategoryId != id);
    }

    public int CountNewsArticles(short id)
    {
        using var db = new FUNewsManagementContext();
        return db.NewsArticles.Count(n => n.CategoryId == id);
    }

    public void AddCategory(Category category)
    {
        using var db = new FUNewsManagementContext();
        db.Categories.Add(category);
        db.SaveChanges();
    }

    public void UpdateCategory(Category category)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId)
                       ?? throw new InvalidOperationException("Category not found.");
        existing.CategoryName = category.CategoryName;
        existing.CategoryDesciption = category.CategoryDesciption;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;
        db.SaveChanges();
    }

    public void DeleteCategory(short id)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.Categories.FirstOrDefault(c => c.CategoryId == id)
                       ?? throw new InvalidOperationException("Category not found.");
        db.Categories.Remove(existing);
        db.SaveChanges();
    }
}
