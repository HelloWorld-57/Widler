using PostsService.Application.Interfaces;
using PostsService.Domain.Entities;

namespace PostsService.Infrastructure.Security
{
    public sealed class PostAuthorization : IPostAuthorization
    {
        private readonly ICurrentUser _currentUser;

        public PostAuthorization(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdate(Post post)
        {
            if (_currentUser.IsInRole("admin"))
                return true;

            if (_currentUser.IsInRole("moderator"))
                return true;

            return post.UserId == _currentUser.Id.ToString();
        }

        public bool CanDelete(Post post)
        {
            if (_currentUser.IsInRole("admin"))
                return true;

            if (_currentUser.IsInRole("moderator"))
                return true;

            return post.UserId == _currentUser.Id.ToString();
        }
    }
}
