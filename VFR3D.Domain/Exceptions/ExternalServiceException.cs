namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Base exception for external service failures (weather APIs, NOTAM services, etc.).
/// </summary>
public class ExternalServiceException : DomainException
{
    /// <summary>
    /// Name of the external service that failed.
    /// </summary>
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string userMessage, string? devMessage = null)
        : base(ErrorCodes.ExternalServiceUnavailable, userMessage, devMessage)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string userMessage, Exception innerException)
        : base(ErrorCodes.ExternalServiceUnavailable, userMessage, innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception thrown when the weather service is unavailable or returns an error.
/// </summary>
public class WeatherServiceException : ExternalServiceException
{
    public WeatherServiceException(string message)
        : base("WeatherService", message)
    {
    }

    public WeatherServiceException(string message, Exception innerException)
        : base("WeatherService", message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when the NOTAM service is unavailable or returns an error.
/// </summary>
public class NotamServiceException : ExternalServiceException
{
    public NotamServiceException(string message)
        : base("NotamService", message)
    {
    }

    public NotamServiceException(string message, Exception innerException)
        : base("NotamService", message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when the magnetic variation service is unavailable.
/// </summary>
public class MagneticVariationServiceException : ExternalServiceException
{
    public MagneticVariationServiceException(string message)
        : base("MagneticVariationService", message)
    {
    }

    public MagneticVariationServiceException(string message, Exception innerException)
        : base("MagneticVariationService", message, innerException)
    {
    }
}
