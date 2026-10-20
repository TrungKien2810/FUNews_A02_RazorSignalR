namespace TrungKienRazorPages.Infrastructure;

/// <summary>
/// Reading time next to a byline: a plain 200 words per minute, floored at one minute
/// so a two-sentence brief does not claim "0 min read". Presentation arithmetic only —
/// no rule in the domain depends on it.
/// </summary>
public static class ReadTime
{
    private const int WordsPerMinute = 200;

    public static int Minutes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return 1;

        var words = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling(words / (double)WordsPerMinute));
    }
}
