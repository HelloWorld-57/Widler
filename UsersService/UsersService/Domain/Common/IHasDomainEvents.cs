using UsersService.Domain.Events.v1;

namespace UsersService.Domain.Common
{
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
