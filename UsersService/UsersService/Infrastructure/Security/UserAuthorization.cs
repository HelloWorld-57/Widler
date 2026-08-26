using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;
using static UsersService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace UsersService.Infrastructure.Security
{
    public sealed class UserAuthorization : IUserAuthorization
    {
        private readonly ICurrentUser _currentUser;

        public UserAuthorization(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdate(User user)
        {
            if (_currentUser.IsInRole("admin"))
                return true;

            if (_currentUser.IsInRole("moderator"))
                return true;

            return user.Id == _currentUser.Id.ToString();
        }

        public bool CanDelete(User user)
        {
            if (_currentUser.IsInRole("admin"))
                return true;

            if (_currentUser.IsInRole("moderator"))
                return true;

            return user.Id == _currentUser.Id.ToString();
        }
    }
}
