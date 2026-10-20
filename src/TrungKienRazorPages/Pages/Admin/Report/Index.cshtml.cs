using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories;
using TrungKienRazorPages.ViewModels;

namespace TrungKienRazorPages.Pages.Admin.Report;

/// <summary>Admin: statistic report of news articles by created date in a period, sorted descending.</summary>
public class IndexModel : PageModel
{
    private readonly INewsArticleRepository _newsRepository;

    public IndexModel(INewsArticleRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    [BindProperty(SupportsGet = true)]
    public ReportViewModel Filter { get; set; } = new();

    public NewsReport? Report { get; set; }

    public void OnGet()
    {
        if (!Filter.StartDate.HasValue && !Filter.EndDate.HasValue)
        {
            // First visit: show the current month without validation errors.
            ModelState.Clear();
            var today = DateTime.Today;
            Filter.StartDate = new DateTime(today.Year, today.Month, 1);
            Filter.EndDate = today;
        }

        if (ModelState.IsValid)
        {
            Report = _newsRepository.GetReport(Filter.StartDate!.Value, Filter.EndDate!.Value);
        }
    }

    /// <summary>Real-time refresh of the report results (called by the SignalR client).</summary>
    public IActionResult OnGetResults()
    {
        if (!ModelState.IsValid || !Filter.StartDate.HasValue || !Filter.EndDate.HasValue)
        {
            return BadRequest();
        }
        return Partial("_ReportResults", _newsRepository.GetReport(Filter.StartDate.Value, Filter.EndDate.Value));
    }
}
