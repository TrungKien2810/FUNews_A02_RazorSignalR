using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories;
using TrungKienRazorPages.Infrastructure;

namespace TrungKienRazorPages.Pages.Staff.News;

/// <summary>Staff: news history (articles created by the signed in staff).</summary>
public class HistoryModel : PageModel
{
    private readonly INewsArticleRepository _newsRepository;

    public HistoryModel(INewsArticleRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }

    public List<NewsArticle> Articles { get; set; } = new();

    public void OnGet()
    {
        Articles = Search();
    }

    public IActionResult OnGetList() => Partial("_NewsTable", Search());

    private List<NewsArticle> Search() => _newsRepository.SearchNewsArticles(Keyword, null, null, User.GetAccountId());
}
