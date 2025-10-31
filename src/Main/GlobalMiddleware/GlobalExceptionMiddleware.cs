using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BaseNetCore.Core.src.Main.GlobalMiddleware
{
    /// <summary>
    /// Middleware that handles unhandled exceptions globally and returns a standardized error response.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
               await HandleExceptionAsync(context, ex, context.TraceIdentifier);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, string requestId)
        {
            _logger.LogError(ex, "Unhandled exception - RequestId: {RequestId}", requestId);

            var response = context.Response;
            response.ContentType = "application/json";
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            ApiErrorResponse apiResponse;
            int statusCode;
            if (ex is BaseApplicationException appEx)
            {
                statusCode = (int)appEx.HttpStatus;
                apiResponse = new ApiErrorResponse(requestId, appEx.ErrorCode.Code, appEx.Message, context);
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse = new ApiErrorResponse(requestId, "UNKNOWN", ex.Message, context);
            }
            response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(apiResponse, options);
            await response.WriteAsync(json);

        }
    }
}
