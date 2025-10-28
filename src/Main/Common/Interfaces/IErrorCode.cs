namespace BaseNetCore.Core.src.Main.Common.Interfaces
{
    /// <summary>
    /// Interface for error codes that can be used in exceptions.
    /// Allows applications to define their own custom error codes while maintaining compatibility with BaseNetCore exception handling.
    /// </summary>
    public interface IErrorCode
    {
        /// <summary>
        /// Error code string (e.g., "SYS001", "USR001", "PRD001")
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Error message or description
        /// </summary>
        string Message { get; }
    }
}
