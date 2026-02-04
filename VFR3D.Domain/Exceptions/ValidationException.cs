namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails on input data.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Dictionary of field names to their validation error messages.
    /// </summary>
    public Dictionary<string, List<string>> ValidationErrors { get; }

    public ValidationException(string message)
        : base(ErrorCodes.ValidationError, message)
    {
        ValidationErrors = new Dictionary<string, List<string>>();
    }

    public ValidationException(string fieldName, string errorMessage)
        : base(ErrorCodes.ValidationError, errorMessage)
    {
        ValidationErrors = new Dictionary<string, List<string>>
        {
            { fieldName, new List<string> { errorMessage } }
        };
    }

    public ValidationException(Dictionary<string, List<string>> validationErrors)
        : base(ErrorCodes.ValidationError, "One or more validation errors occurred.")
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when required weather data is missing for calculations.
/// </summary>
public class WeatherDataMissingException : DomainException
{
    public WeatherDataMissingException(string dataType, string location)
        : base(ErrorCodes.WeatherDataMissing, $"Required {dataType} data is missing for {location}.")
    {
    }

    public WeatherDataMissingException(string message)
        : base(ErrorCodes.WeatherDataMissing, message)
    {
    }
}

/// <summary>
/// Exception thrown when performance calculation data is invalid.
/// </summary>
public class InvalidPerformanceDataException : DomainException
{
    public InvalidPerformanceDataException(string message)
        : base(ErrorCodes.InvalidPerformanceData, message)
    {
    }
}
