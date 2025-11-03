using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BaseNetCore.Core.src.Main.Extensions
{
    /// <summary>
    /// Extension methods for configuring automatic model validation.
    /// </summary>
    public static class ModelValidationExtensions
    {
        /// <summary>
        /// Adds automatic model validation with standardized ApiErrorResponse format.
        /// Returns HTTP 400 with validation errors in ApiErrorResponse format.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBaseAutomaticModelValidation(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
              {
                  options.InvalidModelStateResponseFactory = context =>
                  {
                      var errors = context.ModelState.Where(e => e.Value?.Errors.Count > 0)
                                                     .ToDictionary(
                                                              e => e.Key,
                                                              e => e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                                                     );

                      var errorCode = CoreErrorCodes.REQUEST_INVALID;

                      var apiErrorResponse = new ApiErrorResponse
                      {
                          Guid = context.HttpContext.TraceIdentifier,
                          Code = errorCode.Code,
                          Message = errorCode.Message,
                          Path = context.HttpContext.Request.Path,
                          Method = context.HttpContext.Request.Method,
                          Timestamp = DateTime.UtcNow,
                          Errors = errors
                      };

                      return new BadRequestObjectResult(apiErrorResponse);
                  };
              });

            return services;
        }

        /// <summary>
        /// Adds model validation that throws RequestInvalidException on invalid model state.
        /// Exception will be caught by GlobalExceptionMiddleware.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBaseModelValidationWithException(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
              {
                  options.InvalidModelStateResponseFactory = context =>
                  {
                      var errors = context.ModelState.Where(e => e.Value?.Errors.Count > 0)
                                                    .ToDictionary(
                                                         e => e.Key,
                                                         e => e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                                                     );

                      var errorMessages = string.Join("; ", errors.SelectMany(e => e.Value));

                      throw new RequestInvalidException(errorMessages);
                  };
              });

            return services;
        }
    }
}
