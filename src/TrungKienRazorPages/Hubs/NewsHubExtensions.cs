using Microsoft.AspNetCore.SignalR;

namespace TrungKienRazorPages.Hubs;

public static class NewsHubExtensions
{
    /// <param name="action">"created", "updated" or "deleted"</param>
    public static Task BroadcastNewsChangedAsync(this IHubContext<NewsHub> hub, string action, string newsId, string? title, string? by)
        => hub.Clients.All.SendAsync(NewsHub.NewsChangedMethod, new { action, newsId, title, by });
}
