namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Base exception class for all domain-specific exceptions.
/// Provides structured error information for consistent API responses.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Machine-readable error code for client handling.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// User-friendly error message suitable for display.
    /// </summary>
    public string UserMessage { get; }

    protected DomainException(string errorCode, string userMessage, string? devMessage = null)
        : base(devMessage ?? userMessage)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }

    protected DomainException(string errorCode, string userMessage, Exception innerException)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }
}
