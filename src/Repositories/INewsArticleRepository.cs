using BusinessObjects.Models;

namespace Repositories;

public interface INewsArticleRepository
{
    List<NewsArticle> SearchNewsArticles(string? keyword, short? categoryId, bool? status, short? createdById = null, int? tagId = null);
    NewsArticle? GetNewsArticleById(string id);
    List<NewsArticle> GetNewsArticlesByPeriod(DateTime startDate, DateTime endDate);
    NewsReport GetReport(DateTime startDate, DateTime endDate);
    void AddNewsArticle(NewsArticle article, IEnumerable<int> tagIds);
    void UpdateNewsArticle(NewsArticle article, IEnumerable<int> tagIds);
    void DeleteNewsArticle(string id);
}
