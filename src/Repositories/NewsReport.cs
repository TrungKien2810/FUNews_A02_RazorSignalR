using BusinessObjects.Models;

namespace Repositories;

/// <summary>Statistic report of news articles created in a period.</summary>
public class NewsReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<NewsArticle> Articles { get; set; } = new();
    public List<NewsStatistic> ByCategory { get; set; } = new();
    public List<NewsStatistic> ByAuthor { get; set; } = new();
    public List<NewsStatistic> ByDate { get; set; } = new();
    public int Total => Articles.Count;
    public int Active => Articles.Count(a => a.NewsStatus == true);
    public int Inactive => Total - Active;
}
