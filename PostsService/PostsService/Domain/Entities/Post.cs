using PostsService.Domain.Common;
using PostsService.Domain.Events.v1;
using PostsService.Domain.Exceptions;

namespace PostsService.Domain.Entities
{
    public class Post : IHasDomainEvents
    {
        public string Id { get; private set; } = null!;
        public string Caption { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
        private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
        public void ClearDomainEvents() => _domainEvents.Clear();

        private Post() { }

        public Post(string caption, string content, string userId)
        {
            Id = Guid.NewGuid().ToString();
            Caption = caption;
            Content = content;
            UserId = userId;
            CreationDate = DateTime.UtcNow;

            AddDomainEvent(new PostCreatedDomainEvent(Id, userId));
        }

        public void Update(string? caption, string? content)
        {
            if (IsDeleted)
            {
                throw new DomainException("post.deleted", "Cannot update deleted post");
            }

            var isChanged = false;

            if (caption is not null)
            {
                if (string.IsNullOrWhiteSpace(caption))
                {
                    throw new DomainException("post.caption_invalid", "Caption cannot be empty");
                }

                Caption = caption;
                isChanged = true;
            }

            if (content is not null)
            {
                Content = content;
                isChanged = true;
            }

            if (isChanged)
            {
                AddDomainEvent(new PostUpdatedDomainEvent(Id));
            }
        }

        public void Replace(string caption, string content)
        {
            if (IsDeleted)
            {
                throw new DomainException("post.deleted", "Cannot replace deleted post");
            }

            if (string.IsNullOrWhiteSpace(caption))
            {
                throw new DomainException("post.caption_required", "Caption is required");
            }
                
            Caption = caption;
            Content = content;

            AddDomainEvent(new PostUpdatedDomainEvent(Id));
        }

        public void Delete()
        {
            if (IsDeleted)
            {
                throw new DomainException("post.already_deleted", "Already deleted");
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;

            AddDomainEvent(new PostDeletedDomainEvent(Id));
        }
    }
}
