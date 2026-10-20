using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class TagDAO
{
    private static TagDAO? _instance;
    private static readonly object InstanceLock = new();

    private TagDAO() { }

    public static TagDAO Instance
    {
        get
        {
            lock (InstanceLock)
            {
                return _instance ??= new TagDAO();
            }
        }
    }

    public List<Tag> GetTags()
    {
        using var db = new FUNewsManagementContext();
        return db.Tags.AsNoTracking().OrderBy(t => t.TagName).ToList();
    }
}
