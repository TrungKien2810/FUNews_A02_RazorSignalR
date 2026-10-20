using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repositories;

namespace TrungKienRazorPages.Pages;

/// <summary>
/// RSS 2.0 feed of the active stories, so a reader or an aggregator can follow the
/// newsroom without opening the site. The page renders nothing: the handler answers
/// with the XML itself, and the view file exists only because Razor Pages requires one.
/// </summary>
[AllowAnonymous]
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public class FeedModel : PageModel
{
    private const int MaxItems = 25;

    private readonly INewsArticleRepository _newsRepository;

    public FeedModel(INewsArticleRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public IActionResult OnGet()
    {
        var site = $"{Request.Scheme}://{Request.Host}";
        var articles = _newsRepository.SearchNewsArticles(null, null, true).Take(MaxItems);

        var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var buffer = new MemoryStream();
        using (var xml = XmlWriter.Create(buffer, settings))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement("rss");
            xml.WriteAttributeString("version", "2.0");
            xml.WriteStartElement("channel");

            xml.WriteElementString("title", "FUNews — campus newsroom");
            xml.WriteElementString("link", site + Url.Page("/News/Index"));
            xml.WriteElementString("description", "Published stories from every department.");
            xml.WriteElementString("language", "en");

            foreach (var article in articles)
            {
                var link = site + Url.Page("/News/Details", new { id = article.NewsArticleId });

                xml.WriteStartElement("item");
                xml.WriteElementString("title", article.NewsTitle ?? string.Empty);
                xml.WriteElementString("link", link);
                xml.WriteElementString("guid", link);
                xml.WriteElementString("description", article.Headline ?? string.Empty);
                if (article.CreatedDate.HasValue)
                {
                    xml.WriteElementString("pubDate", article.CreatedDate.Value.ToString("R"));
                }
                xml.WriteEndElement();
            }

            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }

        return Content(Encoding.UTF8.GetString(buffer.ToArray()), "application/rss+xml; charset=utf-8");
    }
}
