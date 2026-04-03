namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Base exception for when a requested resource cannot be found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string userMessage, string? devMessage = null)
        : base(errorCode, userMessage, devMessage)
    {
    }

    public NotFoundException(string resourceType, object resourceId)
        : base(ErrorCodes.NotFound, $"{resourceType} was not found with ID '{resourceId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when an aircraft cannot be found.
/// </summary>
public class AircraftNotFoundException : NotFoundException
{
    public AircraftNotFoundException(string aircraftId)
        : base(ErrorCodes.AircraftNotFound, $"Aircraft was not found with ID '{aircraftId}'.")
    {
    }

    public AircraftNotFoundException(string userId, string aircraftId)
        : base(ErrorCodes.AircraftNotFound, $"Aircraft was not found with ID '{aircraftId}' for user '{userId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when a flight cannot be found.
/// </summary>
public class FlightNotFoundException : NotFoundException
{
    public FlightNotFoundException(string flightId)
        : base(ErrorCodes.FlightNotFound, $"Flight was not found with ID '{flightId}'.")
    {
    }

    public FlightNotFoundException(string userId, string flightId)
        : base(ErrorCodes.FlightNotFound, $"Flight was not found with ID '{flightId}' for user '{userId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when an airport cannot be found.
/// </summary>
public class AirportNotFoundException : NotFoundException
{
    public AirportNotFoundException(string identifier)
        : base(ErrorCodes.AirportNotFound, $"Airport was not found with identifier '{identifier}'.")
    {
    }
}

/// <summary>
/// Exception thrown when METAR data cannot be found.
/// </summary>
public class MetarNotFoundException : NotFoundException
{
    public MetarNotFoundException(string stationId)
        : base(ErrorCodes.MetarNotFound, $"METAR data was not found for station '{stationId}'.")
    {
    }

    public MetarNotFoundException(IEnumerable<string> stationIds)
        : base(ErrorCodes.MetarNotFound, $"METAR data was not found for stations: {string.Join(", ", stationIds)}.")
    {
    }
}

/// <summary>
/// Exception thrown when TAF data cannot be found.
/// </summary>
public class TafNotFoundException : NotFoundException
{
    public TafNotFoundException(string stationId)
        : base(ErrorCodes.TafNotFound, $"TAF data was not found for station '{stationId}'.")
    {
    }

    public TafNotFoundException(IEnumerable<string> stationIds)
        : base(ErrorCodes.TafNotFound, $"TAF data was not found for stations: {string.Join(", ", stationIds)}.")
    {
    }
}

/// <summary>
/// Exception thrown when a performance profile cannot be found.
/// </summary>
public class PerformanceProfileNotFoundException : NotFoundException
{
    public PerformanceProfileNotFoundException(string profileId)
        : base(ErrorCodes.PerformanceProfileNotFound, $"Performance profile was not found with ID '{profileId}'.")
    {
    }

    public PerformanceProfileNotFoundException(string aircraftId, string profileId)
        : base(ErrorCodes.PerformanceProfileNotFound, $"Performance profile was not found with ID '{profileId}' for aircraft '{aircraftId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when a weight and balance profile cannot be found.
/// </summary>
public class WeightBalanceProfileNotFoundException : NotFoundException
{
    public WeightBalanceProfileNotFoundException(string profileId)
        : base(ErrorCodes.WeightBalanceProfileNotFound, $"Weight and balance profile was not found with ID '{profileId}'.")
    {
    }

    public WeightBalanceProfileNotFoundException(string aircraftId, string profileId)
        : base(ErrorCodes.WeightBalanceProfileNotFound, $"Weight and balance profile was not found with ID '{profileId}' for aircraft '{aircraftId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when an aircraft document cannot be found.
/// </summary>
public class DocumentNotFoundException : NotFoundException
{
    public DocumentNotFoundException(string documentId)
        : base(ErrorCodes.DocumentNotFound, $"Document was not found with ID '{documentId}'.")
    {
    }

    public DocumentNotFoundException(string aircraftId, string documentId)
        : base(ErrorCodes.DocumentNotFound, $"Document was not found with ID '{documentId}' for aircraft '{aircraftId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when an airport diagram cannot be found.
/// </summary>
public class AirportDiagramNotFoundException : NotFoundException
{
    public AirportDiagramNotFoundException(string airportId)
        : base(ErrorCodes.AirportDiagramNotFound, $"Airport diagram was not found for airport '{airportId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when a chart supplement cannot be found.
/// </summary>
public class ChartSupplementNotFoundException : NotFoundException
{
    public ChartSupplementNotFoundException(string airportId)
        : base(ErrorCodes.ChartSupplementNotFound, $"Chart supplement was not found for airport '{airportId}'.")
    {
    }
}
