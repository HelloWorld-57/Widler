using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Observability
{
    public sealed class CorrelationContext : ICorrelationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationContext(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CorrelationId =>
            _httpContextAccessor.HttpContext?.Request.Headers[CorrelationHeaders.CorrelationId].FirstOrDefault()
            ?? _httpContextAccessor.HttpContext?.TraceIdentifier
            ?? string.Empty;
    }
}
