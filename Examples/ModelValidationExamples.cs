using BaseNetCore.Core.src.Main.Extensions;
using Microsoft.AspNetCore.Builder;

namespace BaseNetCore.Core.Examples
{
    /// <summary>
    /// Examples of how to use automatic model validation extensions in your ASP.NET Core application.
    /// </summary>
    internal class ModelValidationExamples
    {
        /// <summary>
        /// Example 1: Configure automatic model validation with ApiErrorResponse format (Recommended).
        /// This returns HTTP 400 with standardized error response including validation details.
        /// </summary>
        public void Example1_AutomaticModelValidationWithApiErrorResponse()
        {
            var builder = WebApplication.CreateBuilder();

            // Add automatic model validation with ApiErrorResponse format
            // Returns: { guid, code, message, path, method, timestamp, errors }
            builder.Services.AddAutomaticModelValidation();

            var app = builder.Build();
            app.Run();
        }

        /// <summary>
        /// Example 2: Configure model validation that throws RequestInvalidException.
        /// The exception will be caught by GlobalExceptionMiddleware.
        /// </summary>
        public void Example2_ModelValidationWithException()
        {
            var builder = WebApplication.CreateBuilder();

            // Add model validation with exception
            // Throws RequestInvalidException which is handled by GlobalExceptionMiddleware
            builder.Services.AddModelValidationWithException();

            // Don't forget to add GlobalExceptionMiddleware
            var app = builder.Build();
            app.UseMiddleware<BaseNetCore.Core.src.Main.Middleware.GlobalExceptionMiddleware>();

            app.Run();
        }
    }
}
