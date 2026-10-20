using BusinessObjects.Models;

namespace Repositories;

public interface ITagRepository
{
    List<Tag> GetTags();
}
