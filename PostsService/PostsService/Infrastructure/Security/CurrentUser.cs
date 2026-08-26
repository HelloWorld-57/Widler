using PostsService.Application.Interfaces;

namespace PostsService.Infrastructure.Security
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-User-Id") == true;

        public Guid Id
        {
            get
            {
                var value = GetHeader("X-User-Id");

                if (!Guid.TryParse(value, out var id))
                {
                    throw new InvalidOperationException("Current user id is missing or invalid.");
                }

                return id;
            }
        }

        public string Username => GetHeader("X-User-Name") ?? string.Empty;

        public IReadOnlyCollection<string> Roles
        {
            get
            {
                var value = GetHeader("X-User-Roles");

                if (string.IsNullOrWhiteSpace(value))
                {
                    return [];
                }

                return value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();
            }
        }

        public bool IsInRole(string role)
        {
            return Roles.Contains( role, StringComparer.OrdinalIgnoreCase);
        }

        private string? GetHeader(string name)
        {
            return _httpContextAccessor
                .HttpContext?
                .Request
                .Headers[name]
                .FirstOrDefault();
        }
    }
}
