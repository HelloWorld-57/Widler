using UsersService.Domain.Entities;
using static UsersService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace UsersService.Application.Interfaces
{
    public interface IUserAuthorization
    {
        bool CanUpdate(User user);
        bool CanDelete(User user);
    }
}
