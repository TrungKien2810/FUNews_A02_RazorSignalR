using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    public List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status, short? createdById = null, int? tagId = null)
        => NewsArticleDAO.Instance.SearchNewsArticles(keyword, categoryId, status, createdById, tagId);
    public NewsArticle? GetNewsArticleById(string id) => NewsArticleDAO.Instance.GetNewsArticleById(id);
    public List<NewsArticle> GetNewsArticlesByPeriod(DateTime startDate, DateTime endDate) => NewsArticleDAO.Instance.GetNewsArticlesByPeriod(startDate, endDate);

    /// <summary>Statistic report of news created in a period (sorted by created date descending).</summary>
    public NewsReport GetReport(DateTime startDate, DateTime endDate)
    {
        var articles = NewsArticleDAO.Instance.GetNewsArticlesByPeriod(startDate, endDate);

        static List<NewsStatistic> Group(IEnumerable<NewsArticle> source, Func<NewsArticle, string> key) =>
            source.GroupBy(key)
                .Select(g => new NewsStatistic
                {
                    GroupName = g.Key,
                    Total = g.Count(),
                    Active = g.Count(a => a.NewsStatus == true),
                    Inactive = g.Count(a => a.NewsStatus != true)
                })
                .OrderByDescending(s => s.Total)
                .ThenBy(s => s.GroupName)
                .ToList();

        return new NewsReport
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Articles = articles, // already sorted by CreatedDate descending
            ByCategory = Group(articles, a => a.Category?.CategoryName ?? "(No category)"),
            ByAuthor = Group(articles, a => a.CreatedBy?.AccountName ?? "(Unknown)"),
            ByDate = articles
                .GroupBy(a => a.CreatedDate?.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new NewsStatistic
                {
                    GroupName = g.Key?.ToString("dd/MM/yyyy") ?? "(No date)",
                    Total = g.Count(),
                    Active = g.Count(a => a.NewsStatus == true),
                    Inactive = g.Count(a => a.NewsStatus != true)
                })
                .ToList()
        };
    }

    public void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => NewsArticleDAO.Instance.AddNewsArticle(article, tagIds);
    public void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds) => NewsArticleDAO.Instance.UpdateNewsArticle(article, tagIds);
    public void DeleteNewsArticle(string id) => NewsArticleDAO.Instance.DeleteNewsArticle(id);
}
