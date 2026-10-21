using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Repositories;
using TrungKienRazorPages.Hubs;
using TrungKienRazorPages.Infrastructure;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Staff.News;

/// <summary>
/// Staff: manage news articles including tags (CRUD + search).
/// Every create / update / delete is broadcast to all connected clients with SignalR (NewsHub).
/// </summary>
public class IndexModel : PageModel
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IHubContext<NewsHub> _hub;

    public IndexModel(INewsArticleRepository newsRepository, ICategoryRepository categoryRepository,
        ITagRepository tagRepository, IHubContext<NewsHub> hub)
    {
        _newsRepository = newsRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _hub = hub;
    }

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public short? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? TagId { get; set; }

    public List<NewsArticle> Articles { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();

    public void OnGet()
    {
        Articles = Search();
        Categories = _categoryRepository.GetCategories();
        Tags = _tagRepository.GetTags();
    }

    /// <summary>Real-time refresh: returns only the news table (called by the SignalR client).</summary>
    public IActionResult OnGetList() => Partial("_NewsTable", Search());

    public IActionResult OnGetDetails(string id)
    {
        var article = _newsRepository.GetNewsArticleById(id);
        if (article == null) return NotFound();
        return Partial("_NewsDetails", article);
    }

    public IActionResult OnGetCreate() => FormPartial(new NewsFormViewModel(), null);

    public async Task<IActionResult> OnPostCreateAsync(NewsFormViewModel input)
    {
        ValidateCategory(input, null);
        if (!ModelState.IsValid)
        {
            return FormPartial(input, null);
        }

        var now = DateTime.Now;
        var article = new NewsArticle
        {
            NewsTitle = input.NewsTitle.Trim(),
            Headline = input.Headline.Trim(),
            NewsContent = input.NewsContent.Trim(),
            NewsSource = input.NewsSource?.Trim(),
            CategoryId = input.CategoryId,
            NewsStatus = input.NewsStatus,
            CreatedById = User.GetAccountId(),
            UpdatedById = User.GetAccountId(),
            CreatedDate = now,
            ModifiedDate = now
        };
        _newsRepository.AddNewsArticle(article, input.SelectedTagIds);
        await _hub.BroadcastNewsChangedAsync("created", article.NewsArticleId, article.NewsTitle, User.Identity?.Name);

        TempData["Success"] = $"News \"{input.NewsTitle}\" was created.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetEdit(string id)
    {
        var article = _newsRepository.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        return FormPartial(new NewsFormViewModel
        {
            NewsArticleId = article.NewsArticleId,
            NewsTitle = article.NewsTitle ?? string.Empty,
            Headline = article.Headline,
            NewsContent = article.NewsContent ?? string.Empty,
            NewsSource = article.NewsSource,
            CategoryId = article.CategoryId,
            NewsStatus = article.NewsStatus == true,
            SelectedTagIds = article.Tags.Select(t => t.TagId).ToList()
        }, article.CategoryId);
    }

    public async Task<IActionResult> OnPostEditAsync(NewsFormViewModel input)
    {
        var article = string.IsNullOrEmpty(input.NewsArticleId) ? null : _newsRepository.GetNewsArticleById(input.NewsArticleId);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();

        ValidateCategory(input, article.CategoryId);
        if (!ModelState.IsValid)
        {
            return FormPartial(input, article.CategoryId);
        }

        article.NewsTitle = input.NewsTitle.Trim();
        article.Headline = input.Headline.Trim();
        article.NewsContent = input.NewsContent.Trim();
        article.NewsSource = input.NewsSource?.Trim();
        article.CategoryId = input.CategoryId;
        article.NewsStatus = input.NewsStatus;
        article.UpdatedById = User.GetAccountId();
        article.ModifiedDate = DateTime.Now;
        _newsRepository.UpdateNewsArticle(article, input.SelectedTagIds);
        await _hub.BroadcastNewsChangedAsync("updated", article.NewsArticleId, article.NewsTitle, User.Identity?.Name);

        TempData["Success"] = $"News \"{input.NewsTitle}\" was updated.";
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetDelete(string id)
    {
        var article = _newsRepository.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        return Partial("_DeleteNews", article);
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var article = _newsRepository.GetNewsArticleById(id);
        if (article == null) return NotFound();
        if (!IsOwner(article)) return Forbid();
        _newsRepository.DeleteNewsArticle(id);
        await _hub.BroadcastNewsChangedAsync("deleted", article.NewsArticleId, article.NewsTitle, User.Identity?.Name);

        TempData["Success"] = $"News \"{article.NewsTitle}\" was deleted.";
        return new JsonResult(new { success = true });
    }

    private List<NewsArticle> Search() => _newsRepository.SearchNewsArticles(Keyword, CategoryId, Status, null, TagId);

    /// <summary>
    /// A staff account manages the articles it wrote. "View news history created by him/her"
    /// only makes sense if authorship is the unit of ownership, and a newsroom where any
    /// staff member can silently rewrite another's byline is a data-integrity bug, not a
    /// permission shortcut. Enforced on the server, not only by hiding the buttons.
    /// </summary>
    private bool IsOwner(NewsArticle article) => article.CreatedById == User.GetAccountId();

    /// <summary>New news must use an active category; an existing article may keep its current one.</summary>
    private void ValidateCategory(NewsFormViewModel input, short? currentCategoryId)
    {
        if (!input.CategoryId.HasValue) return;
        var category = _categoryRepository.GetCategoryById(input.CategoryId.Value);
        if (category == null)
        {
            ModelState.AddModelError(nameof(input.CategoryId), "Category does not exist.");
        }
        else if (category.IsActive != true && category.CategoryId != currentCategoryId)
        {
            ModelState.AddModelError(nameof(input.CategoryId), "Category is inactive.");
        }
    }

    private PartialViewResult FormPartial(NewsFormViewModel model, short? currentCategoryId)
    {
        model.CategoryOptions = _categoryRepository.GetCategories()
            .Where(c => c.IsActive == true || c.CategoryId == currentCategoryId)
            .Select(c => new SelectListItem(c.CategoryName, c.CategoryId.ToString()))
            .ToList();
        model.TagOptions = _tagRepository.GetTags();
        return Partial("_NewsForm", model);
    }
}
