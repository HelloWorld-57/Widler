using Serilog.Context;
using System.Diagnostics;

namespace ApiGateway.Infrastructure.Observability
{
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

            //var correlationId = context.Request.Headers[CorrelationHeaders.CorrelationId].FirstOrDefault(); // не доверяем

            //if (string.IsNullOrWhiteSpace(correlationId))
            //{
            //    correlationId = Guid.NewGuid().ToString();
            //}

            var correlationId = Guid.NewGuid().ToString();

            context.Items[CorrelationHeaders.CorrelationId] = correlationId;

            context.Response.Headers[CorrelationHeaders.CorrelationId] = correlationId;

            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
