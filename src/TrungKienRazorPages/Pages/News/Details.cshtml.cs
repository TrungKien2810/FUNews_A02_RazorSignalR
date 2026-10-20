using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories;

namespace TrungKienRazorPages.Pages.News;

[AllowAnonymous]
public class DetailsModel : PageModel
{
    private readonly INewsArticleRepository _newsRepository;

    public DetailsModel(INewsArticleRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public NewsArticle Article { get; set; } = null!;

    /// <summary>What to read next: same section first, then shared tags. Never the article itself.</summary>
    public List<NewsArticle> Related { get; set; } = new();

    public IActionResult OnGet(string id)
    {
        var article = _newsRepository.GetNewsArticleById(id);
        if (article == null || article.NewsStatus != true)
        {
            return NotFound();
        }
        Article = article;
        Related = _newsRepository.GetRelatedArticles(
            article.NewsArticleId,
            article.CategoryId,
            article.Tags.Select(t => t.TagId).ToList(),
            3);
        return Page();
    }
}
