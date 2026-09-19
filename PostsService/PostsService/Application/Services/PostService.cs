using PostsService.Application.Commands;
using PostsService.Application.Exceptions;
using PostsService.Application.Interfaces;
using PostsService.Application.Validation;
using PostsService.Domain.Entities;
using PostsService.DTOs.Response;
using PostsService.Infrastructure.Telemetry;

namespace PostsService.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IValidatorRunner _validator;
        private readonly ICurrentUser _currentUser;
        private readonly IPostAuthorization _authorization;

        public PostService(IPostRepository repo, IValidatorRunner validator, ICurrentUser currentUser, IPostAuthorization authorization)
        {
            _repo = repo;
            _validator = validator;
            _currentUser = currentUser;
            _authorization = authorization;
        }

        //if (currentUser.IsInRole("admin")){}
        //if (currentUser.IsInRole("moderator"))

        //if (post.AuthorId != currentUser.Id)
        //{
        //    throw new ForbiddenException();
        //}

        public async Task<IReadOnlyCollection<PostResponse>> GetAllAsync()
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.GetAll");
            
            var posts = await _repo.GetAllAsync();

            return posts.Select(MapToResponse).ToList();
        }

        public async Task<PostResponse> GetByIdAsync(string id)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.GetById");
            activity?.SetTag("post.id", id);

            var post = await _repo.GetByIdAsync(id);

            if (post == null)
            {
                throw new NotFoundException(nameof(Post), id);
            }

            return MapToResponse(post);
        }

        public async Task<string> CreateAsync(CreatePostCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Create");

            await _validator.ValidateAsync(cmd);

            var post = new Post(
                cmd.Caption,
                cmd.Content,
                _currentUser.Id.ToString()
            );

            await _repo.AddAsync(post);
            await _repo.SaveChangesAsync();

            return post.Id;
        }

        public async Task UpdateAsync(UpdatePostCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Update");
            activity?.SetTag("post.id", cmd.PostId);

            await _validator.ValidateAsync(cmd);

            var post = await _repo.GetByIdAsync(cmd.PostId)
                ?? throw new NotFoundException(nameof(Post), cmd.PostId);

            if (!_authorization.CanUpdate(post))
            {
                throw new ForbiddenException();
            }

            post.Update(cmd.Caption, cmd.Content);

            await _repo.SaveChangesAsync();
        }

        public async Task ReplaceAsync(ReplacePostCommand cmd)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Replace");
            activity?.SetTag("post.id", cmd.PostId);

            await _validator.ValidateAsync(cmd);

            var post = await _repo.GetByIdAsync(cmd.PostId) 
                ?? throw new NotFoundException(nameof(Post), cmd.PostId);

            if (!_authorization.CanUpdate(post))
            {
                throw new ForbiddenException();
            }

            post.Replace(cmd.Caption, cmd.Content);

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            using var activity = Tracing.ActivitySource.StartActivity("PostService.Delete");
            activity?.SetTag("post.id", id);

            var post = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Post), id);
            
            if (!_authorization.CanDelete(post))
            {
                throw new ForbiddenException();
            }

            post.Delete();

            await _repo.SaveChangesAsync();
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
