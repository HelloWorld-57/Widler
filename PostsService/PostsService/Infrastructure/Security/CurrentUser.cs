using PostsService.Application.Interfaces;
using System.Security.Claims;

namespace PostsService.Infrastructure.Security
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;


        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public Guid Id
        {
            get
            {
                var value = Principal?.FindFirstValue("sub");

                if (!Guid.TryParse(value, out var id))
                {
                    throw new InvalidOperationException("Current user id is missing or invalid.");
                }

                return id;
            }
        }

        public string Username => Principal?.FindFirstValue("preferred_username") ?? string.Empty;

        public string Email => Principal?.FindFirstValue("email") ?? string.Empty;

        public IReadOnlyCollection<string> Roles =>
            Principal?
                .FindAll("roles")
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray()
            ?? [];

        public bool IsInRole(string role)
        {
            return Principal?.IsInRole(role) == true;
        }
    }
}
