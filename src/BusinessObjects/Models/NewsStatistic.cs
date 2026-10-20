namespace BusinessObjects.Models;

/// <summary>One row of a grouped statistic report (e.g. number of news per category).</summary>
public class NewsStatistic
{
    public string GroupName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
}
