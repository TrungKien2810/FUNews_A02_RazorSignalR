using Microsoft.AspNetCore.SignalR;

namespace TrungKienRazorPages.Hubs;

/// <summary>
/// Real-time hub. When a staff creates / updates / deletes a news article the server broadcasts
/// "NewsChanged" to every connected client, which then refreshes its news list without reloading.
/// </summary>
public class NewsHub : Hub
{
    public const string Path = "/newsHub";
    public const string NewsChangedMethod = "NewsChanged";
}
