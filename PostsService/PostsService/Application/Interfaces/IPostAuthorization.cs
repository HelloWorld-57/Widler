using PostsService.Domain.Entities;

namespace PostsService.Application.Interfaces
{
    public interface IPostAuthorization
    {
        bool CanUpdate(Post post);
        bool CanDelete(Post post);
    }
}
