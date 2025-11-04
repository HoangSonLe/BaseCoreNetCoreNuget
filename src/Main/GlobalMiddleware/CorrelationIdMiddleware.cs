using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BaseNetCore.Core.src.Main.GlobalMiddleware
{
    /// <summary>
    /// Middleware to add Correlation ID to each request for distributed tracing.
    /// Works seamlessly with Serilog structured logging.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get correlation ID from request header or generate a new one
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            // Store in HttpContext for access throughout the request
            context.Items["CorrelationId"] = correlationId;

            // Add to response headers for client tracking
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            // Push to Serilog LogContext - will be included in all logs during this request
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
