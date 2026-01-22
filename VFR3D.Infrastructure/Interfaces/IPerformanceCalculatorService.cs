using VFR3D.Infrastructure.Dtos.Performance;

namespace VFR3D.Infrastructure.Interfaces;

public interface IPerformanceCalculatorService
{
    /// <summary>
    /// Calculates crosswind components for all runways at an airport using current METAR data
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Crosswind data for all runway ends</returns>
    Task<AirportCrosswindResponseDto> GetCrosswindForAirportAsync(string icaoCodeOrIdent);

    /// <summary>
    /// Calculates crosswind components using manual parameters
    /// </summary>
    /// <param name="request">Crosswind calculation parameters</param>
    /// <returns>Calculated crosswind components</returns>
    CrosswindCalculationResponseDto CalculateCrosswind(CrosswindCalculationRequestDto request);

    /// <summary>
    /// Calculates density altitude for an airport using current METAR data
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <param name="request">Optional parameter overrides</param>
    /// <returns>Density altitude calculation result</returns>
    Task<DensityAltitudeResponseDto> GetDensityAltitudeForAirportAsync(
        string icaoCodeOrIdent,
        AirportDensityAltitudeRequestDto? request = null);

    /// <summary>
    /// Calculates density altitude using manual parameters
    /// </summary>
    /// <param name="request">Density altitude calculation parameters</param>
    /// <returns>Calculated density altitude</returns>
    DensityAltitudeResponseDto CalculateDensityAltitude(DensityAltitudeRequestDto request);
}
