using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Caching
{
    public sealed class RedisUserCache : IUserCache
    {
        private const string KeyPrefix = "user:";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisUserCache(IDistributedCache cache)
        {
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public async Task<User?> GetAsync(string userId, CancellationToken ct = default)
        {
            var key = BuildKey(userId);

            var json = await _cache.GetStringAsync(key, ct);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var cachedUser = JsonSerializer.Deserialize<UserCacheModel>(json, _jsonOptions);

            if (cachedUser is null)
            {
                return null;
            }

            return MapToUser(cachedUser);
        }

        public async Task SetAsync(User user, CancellationToken ct = default)
        {
            var key = BuildKey(user.Id);

            var model = MapToCacheModel(user);

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

        private static UserCacheModel MapToCacheModel(User user)
        {
            return new UserCacheModel(
                user.Id,
                user.Email,
                user.Username,
                user.Name,
                user.SecondName,
                user.BirthDate,
                user.CreationDate,
                user.IsEnabled,
                user.IsDeleted,
                user.DeletedAt);
        }

        private static User MapToUser(UserCacheModel model)
        {
            return User.CreateFromCache(
                model.Id,
                model.Email,
                model.Username,
                model.Name,
                model.SecondName,
                model.BirthDate,
                model.CreationDate,
                model.IsEnabled,
                model.IsDeleted,
                model.DeletedAt);
        }
    }
}
