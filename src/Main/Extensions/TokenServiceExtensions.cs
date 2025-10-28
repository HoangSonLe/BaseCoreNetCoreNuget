using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BaseNetCore.Core.src.Main.Extensions
{
    /// <summary>
    /// Extension methods for configuring JWT authentication and token services.
    /// </summary>
    public static class TokenServiceExtensions
    {
        /// <summary>
        /// Adds JWT token service to the service collection.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="sectionName">Configuration section name (default: "TokenSettings")</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddTokenService(this IServiceCollection services, IConfiguration configuration, string sectionName = "TokenSettings")
        {
            // Bind and validate TokenSettings
            services.Configure<TokenSettings>(configuration.GetSection(sectionName));

            // Register TokenService
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        /// <summary>
        /// Adds JWT authentication with token service to the service collection.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="sectionName">Configuration section name (default: "TokenSettings")</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, string sectionName = "TokenSettings")
        {
            // Add token service first
            services.AddTokenService(configuration, sectionName);

            // Get token settings
            var tokenSettings = configuration.GetSection(sectionName).Get<TokenSettings>();

            if (tokenSettings == null || string.IsNullOrEmpty(tokenSettings.SecretKey))
            {
                throw new InvalidOperationException($"TokenSettings section '{sectionName}' is not configured properly in appsettings.json");
            }

            var key = Encoding.UTF8.GetBytes(tokenSettings.SecretKey);

            // Configure JWT authentication
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                     .AddJwtBearer(options =>
                          {
                              options.RequireHttpsMetadata = false; // Set to true in production
                              options.SaveToken = true;
                              options.TokenValidationParameters = new TokenValidationParameters
                              {
                                  ValidateIssuerSigningKey = true,
                                  IssuerSigningKey = new SymmetricSecurityKey(key),
                                  ValidateIssuer = !string.IsNullOrEmpty(tokenSettings.Issuer),
                                  ValidIssuer = tokenSettings.Issuer,
                                  ValidateAudience = !string.IsNullOrEmpty(tokenSettings.Audience),
                                  ValidAudience = tokenSettings.Audience,
                                  ValidateLifetime = true,
                                  ClockSkew = TimeSpan.Zero
                              };

                              // Handle authentication events
                              options.Events = new JwtBearerEvents
                              {
                                  OnAuthenticationFailed = context =>
                                  {
                                      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                      context.Response.ContentType = "application/json";

                                      var errorCode = CoreErrorCodes.TOKEN_INVALID;

                                      if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                      {
                                          context.Response.Headers.Add("Token-Expired", "true");
                                      }
                                      else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                                      {
                                          errorCode = CoreErrorCodes.TOKEN_INVALID;
                                      }

                                      var apiErrorResponse = new ApiErrorResponse
                                      {
                                          Guid = context.HttpContext.TraceIdentifier,
                                          Code = errorCode.Code,
                                          Message = errorCode.Message,
                                          Path = context.HttpContext.Request.Path,
                                          Method = context.HttpContext.Request.Method,
                                          Timestamp = DateTime.UtcNow
                                      };

                                      var jsonOptions = new JsonSerializerOptions
                                      {
                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                      };
                                      var json = JsonSerializer.Serialize(apiErrorResponse, jsonOptions);

                                      return context.Response.WriteAsync(json);
                                  },
                                  OnChallenge = context =>
                                  {
                                      // Skip default behavior
                                      context.HandleResponse();

                                      if (!context.Response.HasStarted)
                                      {
                                          context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                          context.Response.ContentType = "application/json";

                                          var errorCode = CoreErrorCodes.SYSTEM_AUTHORIZATION;

                                          var apiErrorResponse = new ApiErrorResponse
                                          {
                                              Guid = context.HttpContext.TraceIdentifier,
                                              Code = errorCode.Code,
                                              Message = errorCode.Message,
                                              Path = context.HttpContext.Request.Path,
                                              Method = context.HttpContext.Request.Method,
                                              Timestamp = DateTime.UtcNow
                                          };

                                          var jsonOptions = new JsonSerializerOptions
                                          {
                                              PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                          };
                                          var json = JsonSerializer.Serialize(apiErrorResponse, jsonOptions);

                                          return context.Response.WriteAsync(json);
                                      }

                                      return Task.CompletedTask;
                                  },
                                  OnForbidden = context =>
                                  {
                                      context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                      context.Response.ContentType = "application/json";

                                      var errorCode = CoreErrorCodes.FORBIDDEN;

                                      var apiErrorResponse = new ApiErrorResponse
                                      {
                                          Guid = context.HttpContext.TraceIdentifier,
                                          Code = errorCode.Code,
                                          Message = errorCode.Message,
                                          Path = context.HttpContext.Request.Path,
                                          Method = context.HttpContext.Request.Method,
                                          Timestamp = DateTime.UtcNow
                                      };

                                      var jsonOptions = new JsonSerializerOptions
                                      {
                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                      };
                                      var json = JsonSerializer.Serialize(apiErrorResponse, jsonOptions);

                                      return context.Response.WriteAsync(json);
                                  }
                              };
                          });

            return services;
        }
    }
}
