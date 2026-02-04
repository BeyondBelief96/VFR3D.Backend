namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Centralized error codes for consistent error identification across the API.
/// </summary>
public static class ErrorCodes
{
    // General
    public const string InternalError = "INTERNAL_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";

    // Aircraft
    public const string AircraftNotFound = "AIRCRAFT_NOT_FOUND";
    public const string AircraftDuplicateTailNumber = "AIRCRAFT_DUPLICATE_TAIL_NUMBER";
    public const string AircraftInUse = "AIRCRAFT_IN_USE";

    // Aircraft Performance Profiles
    public const string PerformanceProfileNotFound = "PERFORMANCE_PROFILE_NOT_FOUND";
    public const string PerformanceProfileDuplicateName = "PERFORMANCE_PROFILE_DUPLICATE_NAME";

    // Weight & Balance Profiles
    public const string WeightBalanceProfileNotFound = "WEIGHT_BALANCE_PROFILE_NOT_FOUND";
    public const string WeightBalanceProfileDuplicateName = "WEIGHT_BALANCE_PROFILE_DUPLICATE_NAME";

    // Aircraft Documents
    public const string DocumentNotFound = "DOCUMENT_NOT_FOUND";

    // Flights
    public const string FlightNotFound = "FLIGHT_NOT_FOUND";

    // Airports
    public const string AirportNotFound = "AIRPORT_NOT_FOUND";
    public const string AirportDiagramNotFound = "AIRPORT_DIAGRAM_NOT_FOUND";

    // Weather
    public const string MetarNotFound = "METAR_NOT_FOUND";
    public const string TafNotFound = "TAF_NOT_FOUND";
    public const string WeatherServiceUnavailable = "WEATHER_SERVICE_UNAVAILABLE";
    public const string WeatherDataMissing = "WEATHER_DATA_MISSING";

    // NOTAMs
    public const string NotamServiceUnavailable = "NOTAM_SERVICE_UNAVAILABLE";

    // Chart Supplements
    public const string ChartSupplementNotFound = "CHART_SUPPLEMENT_NOT_FOUND";

    // Airspace
    public const string AirspaceNotFound = "AIRSPACE_NOT_FOUND";

    // External Services
    public const string ExternalServiceUnavailable = "EXTERNAL_SERVICE_UNAVAILABLE";

    // Performance Calculations
    public const string PerformanceCalculationError = "PERFORMANCE_CALCULATION_ERROR";
    public const string InvalidPerformanceData = "INVALID_PERFORMANCE_DATA";

    // Navigation
    public const string NavlogCalculationError = "NAVLOG_CALCULATION_ERROR";
}
