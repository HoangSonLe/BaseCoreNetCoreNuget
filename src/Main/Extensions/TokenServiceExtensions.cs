using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.Security.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
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

            if (tokenSettings == null || string.IsNullOrEmpty(tokenSettings.RsaPublicKey))
            {
                throw new InvalidOperationException($"TokenSettings section '{sectionName}' is not configured properly in appsettings.json. RsaPublicKey is required.");
            }

            // Show detailed IdentityModel logs (PII). Only enable in development.
            IdentityModelEventSource.ShowPII = true;

            // Load RSA public key for token validation
            var rsa = RSA.Create();
            rsa.ImportFromPem(tokenSettings.RsaPublicKey);
            var key = new RsaSecurityKey(rsa);

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
                                  IssuerSigningKey = key,
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
                                  // Called early: you can inspect the incoming header and token
                                  OnMessageReceived = context =>
                                  {
                                      // Put a breakpoint here to inspect raw incoming token/header
                                      // Example: var token = context.Request.Headers["Authorization"].ToString();
                                      return Task.CompletedTask;
                                  },

                                  // Called when token is succesfully validated by the framework
                                  OnTokenValidated = async context =>
                                  {
                                      string rawToken = null;
                                      var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                                      if (!string.IsNullOrEmpty(authHeader))
                                      {
                                          const string bearerPrefix = "Bearer ";
                                          rawToken = authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                                              ? authHeader.Substring(bearerPrefix.Length).Trim()
                                              : authHeader.Trim();
                                      }
                                      else
                                      {
                                          rawToken = context.Request.Query["access_token"].FirstOrDefault();
                                      }
                                      // Resolve application-provided validator (registered in DI)
                                      var validator = context.HttpContext.RequestServices.GetService(typeof(ITokenValidator))
                                                      as ITokenValidator;

                                      if (validator != null)
                                      {
                                          var ok = await validator.ValidateAsync(context.Principal, rawToken, context.HttpContext);
                                          if (!ok)
                                          {
                                              // Mark token invalid so pipeline triggers 401/OnAuthenticationFailed
                                              context.Fail("Token rejected by application DB validation");
                                              return;
                                          }
                                      }
                                  },
                                  OnAuthenticationFailed = context =>
                                  {
                                      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                      context.Response.ContentType = "application/json";

                                      var errorCode = CoreErrorCodes.TOKEN_INVALID;

                                      // Use 'is' so derived exception types are also caught
                                      if (context.Exception is SecurityTokenExpiredException)
                                      {
                                          context.Response.Headers.Add("Token-Expired", "true");
                                          errorCode = CoreErrorCodes.TOKEN_EXPIRED;
                                      }
                                      else if (context.Exception is SecurityTokenInvalidSignatureException)
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
