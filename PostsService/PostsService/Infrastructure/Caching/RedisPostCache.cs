using Microsoft.Extensions.Caching.Distributed;
using PostsService.Application.Interfaces;
using PostsService.Domain.Entities;
using System.Text.Json;
using static PostsService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace PostsService.Infrastructure.Caching
{
    public class RedisPostCache : IPostCache
    {
        private const string KeyPrefix = "post:";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisPostCache(IDistributedCache cache)
        {
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public async Task<Post?> GetAsync(string postId, CancellationToken ct = default)
        {
            var key = BuildKey(postId);

            var json = await _cache.GetStringAsync(key, ct);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var cachedPost = JsonSerializer.Deserialize<PostCacheModel>(json, _jsonOptions);

            if (cachedPost is null)
            {
                return null;
            }

            return MapToPost(cachedPost);
        }

        public async Task SetAsync(Post post, CancellationToken ct = default)
        {
            var key = BuildKey(post.Id);

            var model = MapToCacheModel(post);

            var json = JsonSerializer.Serialize(model, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheLifetime
            };

            await _cache.SetStringAsync(
                key,
                json,
                options,
                ct);
        }

        public Task RemoveAsync(string userId, CancellationToken ct = default)
        {
            var key = BuildKey(userId);

            return _cache.RemoveAsync(key, ct);
        }

        private static string BuildKey(string userId)
        {
            return $"{KeyPrefix}{userId}";
        }

        private static PostCacheModel MapToCacheModel(Post post)
        {
            return new PostCacheModel(
                post.Id,
                post.Caption,
                post.Content,
                post.UserId,
                post.CreationDate,
                post.IsDeleted,
                post.DeletedAt);
        }

        private static Post MapToPost(PostCacheModel model)
        {
            return Post.CreateFromCache(
                model.Id,
                model.Caption,
                model.Content,
                model.UserId,
                model.CreationDate,
                model.IsDeleted,
                model.DeletedAt);
        }
    }
}
