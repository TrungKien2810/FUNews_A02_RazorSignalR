using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories;

namespace TrungKienRazorPages.Pages.News;

/// <summary>Public news page: anyone (anonymous, Lecturer, ...) can view ACTIVE news articles.</summary>
[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;

    public IndexModel(INewsArticleRepository newsRepository, ICategoryRepository categoryRepository, ITagRepository tagRepository)
    {
        _newsRepository = newsRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
    }

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public short? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? TagId { get; set; }

    public List<NewsArticle> Articles { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();

    public void OnGet()
    {
        Articles = Search();
        Categories = _categoryRepository.GetActiveCategories();
        Tags = _tagRepository.GetTags();
    }

    /// <summary>Real-time refresh: returns only the news list (called by the SignalR client).</summary>
    public IActionResult OnGetList() => Partial("_NewsCards", Search());

    private List<NewsArticle> Search() => _newsRepository.SearchNewsArticles(Keyword, CategoryId, true, null, TagId);
}
