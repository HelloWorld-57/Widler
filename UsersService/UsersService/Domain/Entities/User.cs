using UsersService.Domain.Exceptions;
using UsersService.Domain.Common;
using UsersService.Domain.Events.v1;

namespace UsersService.Domain.Entities
{
    public class User : IHasDomainEvents
    {
        public string Id { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string SecondName { get; private set; } = null!;
        public string FullName => $"{Name} {SecondName}";
        public string Email { get; private set; } = null!;
        public DateTime BirthDate { get; private set; }
        public int Age => DateTime.Today.Year - BirthDate.Year;
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
        private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
        public void ClearDomainEvents() => _domainEvents.Clear();

        private User() { }

        public User(string name, string secondName, string email, DateTime birthDate)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            SecondName = secondName;
            Email = email;
            BirthDate = birthDate;
            CreationDate = DateTime.UtcNow;

            AddDomainEvent(new UserCreatedDomainEvent(Id));
        }

        public void Update(string? name, string? secondName, string? email, DateTime? birthDate)
        {
            if (IsDeleted)
            {
                throw new DomainException("user.deleted", "Cannot update deleted user");
            }

            var isChanged = false;

            if (name is not null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new DomainException("user.name_invalid", "Name cannot be empty");
                }

                Name = name;
                isChanged = true;
            }

            if (secondName is not null)
            {
                if (string.IsNullOrWhiteSpace(secondName))
                {
                    throw new DomainException("user.secondName_invalid", "SecondName cannot be empty");
                }

                SecondName = secondName;
                isChanged = true;
            }

            if (email is not null)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new DomainException("user.email_invalid", "Email cannot be empty");
                }

                Email = email;
                isChanged = true;
            }

            if (birthDate is not null)
            {
                BirthDate = birthDate.Value;
                isChanged = true;
            }

            if (isChanged)
            {
                AddDomainEvent(new UserUpdatedDomainEvent(Id));
            }
        }

        public void Replace(string name, string secondName, string email, DateTime birthDate)
        {
            if (IsDeleted)
            {
                throw new DomainException("user.deleted", "Cannot replace deleted user");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("user.name_required", "Name is required");
            }

            if (string.IsNullOrWhiteSpace(secondName))
            {
                throw new DomainException("user.secondName_required", "SecondName is required");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainException("user.email_required", "Email is required");
            }

            Name = name;
            SecondName = secondName;
            Email = email;
            BirthDate = birthDate;

            AddDomainEvent(new UserUpdatedDomainEvent(Id));
        }

        public void Delete()
        {
            if (IsDeleted)
            {
                throw new DomainException("user.already_deleted", "Already deleted");
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;

            AddDomainEvent(new UserDeletedDomainEvent(Id));
        }
    }
}
