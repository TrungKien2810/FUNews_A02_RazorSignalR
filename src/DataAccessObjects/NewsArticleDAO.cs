using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class NewsArticleDAO
{
    private static NewsArticleDAO? _instance;
    private static readonly object InstanceLock = new();

    private NewsArticleDAO() { }

    public static NewsArticleDAO Instance
    {
        get
        {
            lock (InstanceLock)
            {
                return _instance ??= new NewsArticleDAO();
            }
        }
    }

    private static IQueryable<NewsArticle> QueryWithDetails(FUNewsManagementContext db) =>
        db.NewsArticles.AsNoTracking()
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags);

    /// <summary>
    /// Searches news articles. Every filter is optional.
    /// </summary>
    public List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status,
        short? createdById = null, int? tagId = null)
    {
        using var db = new FUNewsManagementContext();
        var query = QueryWithDetails(db);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            query = query.Where(n => (n.NewsTitle != null && n.NewsTitle.Contains(key))
                                     || n.Headline.Contains(key)
                                     || (n.NewsContent != null && n.NewsContent.Contains(key))
                                     || (n.NewsSource != null && n.NewsSource.Contains(key)));
        }
        if (categoryId.HasValue)
        {
            query = query.Where(n => n.CategoryId == categoryId.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(n => n.NewsStatus == status.Value);
        }
        if (createdById.HasValue)
        {
            query = query.Where(n => n.CreatedById == createdById.Value);
        }
        if (tagId.HasValue)
        {
            query = query.Where(n => n.Tags.Any(t => t.TagId == tagId.Value));
        }
        return query.OrderByDescending(n => n.CreatedDate).ThenByDescending(n => n.NewsArticleId).ToList();
    }

    public NewsArticle? GetNewsArticleById(string id)
    {
        using var db = new FUNewsManagementContext();
        return QueryWithDetails(db).FirstOrDefault(n => n.NewsArticleId == id);
    }

    /// <summary>News created in [startDate, endDate] (whole days), newest first.</summary>
    public List<NewsArticle> GetNewsArticlesByPeriod(DateTime startDate, DateTime endDate)
    {
        using var db = new FUNewsManagementContext();
        var from = startDate.Date;
        var to = endDate.Date.AddDays(1);
        return QueryWithDetails(db)
            .Where(n => n.CreatedDate >= from && n.CreatedDate < to)
            .OrderByDescending(n => n.CreatedDate)
            .ThenByDescending(n => n.NewsArticleId)
            .ToList();
    }

    /// <summary>
    /// Active stories in the same section, best match first: more shared tags, then
    /// newest. Same section only, so the heading "More from &lt;category&gt;" is literally
    /// true; shared tags decide the order inside it. With no category to go on, tags are
    /// all there is. Drives "More from this section" on the public article page.
    /// </summary>
    public List<NewsArticle> GetRelatedArticles(string articleId, short? categoryId, IReadOnlyCollection<int> tagIds, int take)
    {
        var ids = tagIds.Distinct().ToList();
        using var db = new FUNewsManagementContext();
        var query = QueryWithDetails(db).Where(n => n.NewsStatus == true && n.NewsArticleId != articleId);
        query = categoryId.HasValue
            ? query.Where(n => n.CategoryId == categoryId.Value)
            : query.Where(n => n.Tags.Any(t => ids.Contains(t.TagId)));

        return query.OrderByDescending(n => n.CreatedDate).ToList()
            .OrderByDescending(n => n.Tags.Count(t => ids.Contains(t.TagId)))
            .ThenByDescending(n => n.CreatedDate)
            .Take(take)
            .ToList();
    }

    public void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds)
    {
        using var db = new FUNewsManagementContext();
        // NewsArticleID is an nvarchar key: generate the next numeric id.
        var maxId = db.NewsArticles.Select(n => n.NewsArticleId).AsEnumerable()
            .Select(id => int.TryParse(id, out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();
        article.NewsArticleId = (maxId + 1).ToString();
        var ids = tagIds.ToList();
        article.Tags = db.Tags.Where(t => ids.Contains(t.TagId)).ToList();
        db.NewsArticles.Add(article);
        db.SaveChanges();
    }

    public void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.NewsArticles.Include(n => n.Tags)
                           .FirstOrDefault(n => n.NewsArticleId == article.NewsArticleId)
                       ?? throw new InvalidOperationException("News article not found.");
        existing.NewsTitle = article.NewsTitle;
        existing.Headline = article.Headline;
        existing.NewsContent = article.NewsContent;
        existing.NewsSource = article.NewsSource;
        existing.CategoryId = article.CategoryId;
        existing.NewsStatus = article.NewsStatus;
        existing.UpdatedById = article.UpdatedById;
        existing.ModifiedDate = article.ModifiedDate;

        var ids = tagIds.ToList();
        existing.Tags.Clear();
        foreach (var tag in db.Tags.Where(t => ids.Contains(t.TagId)))
        {
            existing.Tags.Add(tag);
        }
        db.SaveChanges();
    }

    public void DeleteNewsArticle(string id)
    {
        using var db = new FUNewsManagementContext();
        var existing = db.NewsArticles.Include(n => n.Tags).FirstOrDefault(n => n.NewsArticleId == id)
                       ?? throw new InvalidOperationException("News article not found.");
        // Remove NewsTag rows first (no cascade on FK_NewsTag_NewsArticle).
        existing.Tags.Clear();
        db.NewsArticles.Remove(existing);
        db.SaveChanges();
    }
}
