using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Interfaces;
using System.Net;

namespace BaseNetCore.Core.src.Main.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all business logic exceptions in the application.
    /// Provides structured error handling with error codes and HTTP status codes.
    /// Now supports custom error codes via IErrorCode interface.
    /// </summary>
    public class BaseApplicationException : Exception
    {
        public IErrorCode ErrorCode { get; }
        public new string Message { get; }
        public HttpStatusCode HttpStatus { get; }

        /// <summary>
        /// Constructor with IErrorCode support - allows custom error codes
        /// </summary>
        public BaseApplicationException(IErrorCode errorCode, string? message, HttpStatusCode status = HttpStatusCode.BadRequest)
         : base(message ?? errorCode?.Message)
        {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            Message = message ?? errorCode.Message;
            HttpStatus = status;
        }

        /// <summary>
        /// Constructor with string error code - for backward compatibility
        /// </summary>
        public BaseApplicationException(string errorCode, string message, HttpStatusCode status = HttpStatusCode.BadRequest)
            : base(message)
        {
            ErrorCode = new SimpleErrorCode(errorCode, message);
            Message = message;
            HttpStatus = status;
        }

        public BaseApplicationException()
        {
        }

        /// <summary>
        /// Simple implementation of IErrorCode for string-based error codes
        /// </summary>
        private class SimpleErrorCode : IErrorCode
        {
            public string Code { get; }
            public string Message { get; }

            public SimpleErrorCode(string code, string message)
            {
                Code = code;
                Message = message;
            }
        }
    }

    /// <summary>
    /// Exception for system errors (HTTP 500).
    /// </summary>
    public class SystemErrorException : BaseApplicationException
    {
        public SystemErrorException() : base(CoreErrorCodes.SYSTEM_ERROR, CoreErrorCodes.SYSTEM_ERROR.Message, HttpStatusCode.InternalServerError)
        {
        }

        public SystemErrorException(string message) : base(CoreErrorCodes.SYSTEM_ERROR, message, HttpStatusCode.InternalServerError)
        {
        }
    }

    /// <summary>
    /// Exception for data conflicts (HTTP 409).
    /// </summary>
    public class ConflictException : BaseApplicationException
    {
        public ConflictException() : base(CoreErrorCodes.CONFLICT, CoreErrorCodes.CONFLICT.Message, HttpStatusCode.Conflict)
        {
        }

        public ConflictException(string message) : base(CoreErrorCodes.CONFLICT, message, HttpStatusCode.Conflict)
        {
        }
    }

    /// <summary>
    /// Exception for forbidden access (HTTP 403).
    /// </summary>
    public class ForbiddenException : BaseApplicationException
    {
        public ForbiddenException() : base(CoreErrorCodes.FORBIDDEN, CoreErrorCodes.FORBIDDEN.Message, HttpStatusCode.Forbidden)
        {
        }

        public ForbiddenException(string message) : base(CoreErrorCodes.FORBIDDEN, message, HttpStatusCode.Forbidden)
        {
        }
    }

    /// <summary>
    /// Exception for invalid request data (HTTP 400).
    /// </summary>
    public class RequestInvalidException : BaseApplicationException
    {
        public RequestInvalidException() : base(CoreErrorCodes.REQUEST_INVALID, CoreErrorCodes.REQUEST_INVALID.Message, HttpStatusCode.BadRequest)
        {
        }

        public RequestInvalidException(string message) : base(CoreErrorCodes.REQUEST_INVALID, message, HttpStatusCode.BadRequest)
        {
        }
    }

    /// <summary>
    /// Exception for resource not found (HTTP 404).
    /// </summary>
    public class ResourceNotFoundException : BaseApplicationException
    {
        public ResourceNotFoundException() : base(CoreErrorCodes.RESOURCE_NOT_FOUND, CoreErrorCodes.RESOURCE_NOT_FOUND.Message, HttpStatusCode.NotFound)
        {
        }

        public ResourceNotFoundException(string message) : base(CoreErrorCodes.RESOURCE_NOT_FOUND, message, HttpStatusCode.NotFound)
        {
        }
    }

    /// <summary>
    /// Exception for server errors (HTTP 500).
    /// </summary>
    public class ServerErrorException : BaseApplicationException
    {
        public ServerErrorException() : base(CoreErrorCodes.SERVER_ERROR, CoreErrorCodes.SERVER_ERROR.Message, HttpStatusCode.InternalServerError)
        {
        }

        public ServerErrorException(string message) : base(CoreErrorCodes.SERVER_ERROR, message, HttpStatusCode.InternalServerError)
        {
        }
    }

    /// <summary>
    /// Exception for invalid or expired tokens (HTTP 401).
    /// </summary>
    public class TokenInvalidException : BaseApplicationException
    {
        public TokenInvalidException() : base(CoreErrorCodes.TOKEN_INVALID, CoreErrorCodes.TOKEN_INVALID.Message, HttpStatusCode.Unauthorized)
        {
        }

        public TokenInvalidException(string message) : base(CoreErrorCodes.TOKEN_INVALID, message, HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>
    /// Exception for authorization failures (HTTP 403).
    /// </summary>
    public class SystemAuthorizationException : BaseApplicationException
    {
        public SystemAuthorizationException() : base(CoreErrorCodes.SYSTEM_AUTHORIZATION, CoreErrorCodes.SYSTEM_AUTHORIZATION.Message, HttpStatusCode.Forbidden)
        {
        }

        public SystemAuthorizationException(string message) : base(CoreErrorCodes.SYSTEM_AUTHORIZATION, message, HttpStatusCode.Forbidden)
        {
        }
    }

    /// <summary>
    /// Exception for service unavailable (HTTP 503).
    /// </summary>
    public class ServiceUnavailableException : BaseApplicationException
    {
        public ServiceUnavailableException() : base(CoreErrorCodes.SERVICE_UNAVAILABLE, CoreErrorCodes.SERVICE_UNAVAILABLE.Message, HttpStatusCode.ServiceUnavailable)
        {
        }

        public ServiceUnavailableException(string message) : base(CoreErrorCodes.SERVICE_UNAVAILABLE, message, HttpStatusCode.ServiceUnavailable)
        {
        }
    }

    /// <summary>
    /// Exception for bad requests (HTTP 400).
    /// </summary>
    public class BadRequestException : BaseApplicationException
    {
        public BadRequestException() : base(CoreErrorCodes.BAD_REQUEST, CoreErrorCodes.BAD_REQUEST.Message, HttpStatusCode.BadRequest)
        {
        }

        public BadRequestException(string message) : base(CoreErrorCodes.BAD_REQUEST, message, HttpStatusCode.BadRequest)
        {
        }
    }

    /// <summary>
    /// Exception for too many requests (HTTP 429).
    /// </summary>
    public class TooManyRequestsException : BaseApplicationException
    {
        public TooManyRequestsException() : base(CoreErrorCodes.TOO_MANY_REQUESTS, CoreErrorCodes.TOO_MANY_REQUESTS.Message, HttpStatusCode.TooManyRequests)
        {
        }

        public TooManyRequestsException(string message) : base(CoreErrorCodes.TOO_MANY_REQUESTS, message, HttpStatusCode.TooManyRequests)
        {
        }
    }
}
