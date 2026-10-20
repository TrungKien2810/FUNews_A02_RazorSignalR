using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class TagRepository : ITagRepository
{
    public List<Tag> GetTags() => TagDAO.Instance.GetTags();
}
