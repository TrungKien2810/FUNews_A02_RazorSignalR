using Repositories;

namespace TrungKienRazorPages.ViewModels;

/// <summary>
/// The statistic report and the count of the period immediately before it. A type rather
/// than a loose ViewData entry because the report block is also rendered on its own by the
/// SignalR refresh, and a partial that depends on ambient state renders differently
/// depending on who called it.
/// </summary>
public class ReportResultsViewModel
{
    public NewsReport Report { get; init; } = null!;

    /// <summary>Null when there is nothing to compare against.</summary>
    public int? PreviousTotal { get; init; }

    public int? Trend => PreviousTotal.HasValue ? Report.Total - PreviousTotal.Value : null;
}
