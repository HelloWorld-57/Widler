using PostsService.Application.Commands;
using PostsService.Application.Exceptions;
using PostsService.Application.Interfaces;
using PostsService.Application.Validation;
using PostsService.Domain.Entities;
using PostsService.DTOs.Response;
using PostsService.Infrastructure.Telemetry;
using static PostsService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace PostsService.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IValidatorRunner _validator;
        private readonly ICurrentUser _currentUser;
        private readonly IPostAuthorization _authorization;
        private readonly IPostCache _postCache;

        public PostService(
            IPostRepository repo, 
            IValidatorRunner validator, 
            ICurrentUser currentUser, 
            IPostAuthorization authorization,
            IPostCache postCache)
        {
            _repo = repo;
            _validator = validator;
            _currentUser = currentUser;
            _authorization = authorization;
            _postCache = postCache;
        }

        //if (currentUser.IsInRole("admin")){}
        //if (currentUser.IsInRole("moderator"))

        //if (post.AuthorId != currentUser.Id)
        //{
        //    throw new ForbiddenException();
        //}

        public async Task<IReadOnlyCollection<PostResponse>> GetAllAsync(CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.GetAll");
            
            var posts = await _repo.GetAllAsync(ct);

            return posts.Select(MapToResponse).ToList();
        }

        public async Task<PostResponse> GetByIdAsync(string id, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.GetById");
            activity?.SetTag("post.id", id);

            var cachedPost = await _postCache.GetAsync(id, ct);

            if (cachedPost is not null)
            {
                return MapToResponse(cachedPost);
            }

            var post = await _repo.GetByIdAsync(id, ct);

            if (post == null)
            {
                throw new NotFoundException(nameof(Post), id);
            }

            await _postCache.SetAsync(post, ct);

            return MapToResponse(post);
        }

        public async Task<string> CreateAsync(CreatePostCommand cmd, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Create");

            await _validator.ValidateAsync(cmd);

            var post = new Post(
                cmd.Caption,
                cmd.Content,
                _currentUser.Id.ToString()
            );

            await _repo.AddAsync(post);
            await _repo.SaveChangesAsync(ct);

            return post.Id;
        }

        public async Task UpdateAsync(UpdatePostCommand cmd, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Update");
            activity?.SetTag("post.id", cmd.PostId);

            await _validator.ValidateAsync(cmd);

            var post = await _repo.GetByIdAsync(cmd.PostId, ct)
                ?? throw new NotFoundException(nameof(Post), cmd.PostId);

            if (!_authorization.CanUpdate(post))
            {
                throw new ForbiddenException();
            }

            post.Update(cmd.Caption, cmd.Content);

            await _repo.SaveChangesAsync(ct);

            await _postCache.SetAsync(post, ct);
        }

        public async Task ReplaceAsync(ReplacePostCommand cmd, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Replace");
            activity?.SetTag("post.id", cmd.PostId);

            await _validator.ValidateAsync(cmd);

            var post = await _repo.GetByIdAsync(cmd.PostId, ct) 
                ?? throw new NotFoundException(nameof(Post), cmd.PostId);

            if (!_authorization.CanUpdate(post))
            {
                throw new ForbiddenException();
            }

            post.Replace(cmd.Caption, cmd.Content);

            await _repo.SaveChangesAsync(ct);

            await _postCache.SetAsync(post, ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Delete");
            activity?.SetTag("post.id", id);

            var post = await _repo.GetByIdAsync(id, ct)
                ?? throw new NotFoundException(nameof(Post), id);
            
            if (!_authorization.CanDelete(post))
            {
                throw new ForbiddenException();
            }

            post.Delete();

            await _repo.SaveChangesAsync(ct);

            await _postCache.RemoveAsync(id, ct);
        }

        private static PostResponse MapToResponse(Post post)
            => new(
                post.Id,
                post.Caption,
                post.Content,
                post.UserId,
                post.CreationDate
            );
    }
}
