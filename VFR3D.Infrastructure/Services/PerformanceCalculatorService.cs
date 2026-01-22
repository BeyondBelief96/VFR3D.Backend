using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Performance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class PerformanceCalculatorService : IPerformanceCalculatorService
{
    private readonly VFR3DDbContext _context;
    private readonly IMetarService _metarService;
    private readonly ILogger<PerformanceCalculatorService> _logger;

    // Standard atmosphere constants
    private const double StandardPressureInHg = 29.92;
    private const double SeaLevelStandardTempCelsius = 15.0;
    private const double LapseRateCelsiusPerThousandFt = 2.0;
    private const double DensityAltitudeFactor = 120.0;

    public PerformanceCalculatorService(
        VFR3DDbContext context,
        IMetarService metarService,
        ILogger<PerformanceCalculatorService> logger)
    {
        _context = context;
        _metarService = metarService;
        _logger = logger;
    }

    public async Task<AirportCrosswindResponseDto> GetCrosswindForAirportAsync(string icaoCodeOrIdent)
    {
        _logger.LogInformation("Calculating crosswind for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);

        // Get airport data
        var airport = await _context.Airports
            .FirstOrDefaultAsync(a =>
                a.IcaoId == icaoCodeOrIdent.ToUpper() ||
                a.ArptId == icaoCodeOrIdent.ToUpper());

        if (airport == null)
        {
            throw new ResourceNotFoundException(
                $"Airport not found for ICAO code or identifier: {icaoCodeOrIdent}");
        }

        // Get METAR data
        var metar = await _metarService.GetMetarForAirport(icaoCodeOrIdent);

        // Validate METAR has required wind data
        if (metar.WindSpeedKt == null)
        {
            throw new InvalidDataException("METAR missing wind speed data");
        }

        // Get runways for this airport
        var runways = await _context.Runways
            .Include(r => r.RunwayEnds)
            .Where(r => r.SiteNo == airport.SiteNo)
            .ToListAsync();

        // Parse wind direction (handle VRB)
        bool isVariableWind = metar.WindDirDegrees == "VRB" ||
                              string.IsNullOrEmpty(metar.WindDirDegrees) ||
                              !int.TryParse(metar.WindDirDegrees, out _);
        int? windDirection = isVariableWind ? null : int.Parse(metar.WindDirDegrees!);

        var runwayCrosswinds = new List<RunwayCrosswindComponentDto>();

        foreach (var runway in runways)
        {
            foreach (var runwayEnd in runway.RunwayEnds)
            {
                // Skip runway ends without true alignment
                if (runwayEnd.TrueAlignment == null)
                {
                    _logger.LogDebug("Skipping runway end {RunwayEndId} - no true alignment",
                        runwayEnd.RunwayEndId);
                    continue;
                }

                // Convert true heading to magnetic heading
                int magneticHeading = ConvertTrueToMagnetic(
                    runwayEnd.TrueAlignment.Value,
                    airport.MagVarn,
                    airport.MagHemis);

                // Calculate crosswind components
                var components = CalculateCrosswindComponents(
                    windDirection,
                    metar.WindSpeedKt.Value,
                    metar.WindGustKt,
                    magneticHeading,
                    isVariableWind);

                runwayCrosswinds.Add(new RunwayCrosswindComponentDto
                {
                    RunwayEndId = runwayEnd.RunwayEndId,
                    MagneticHeadingDegrees = magneticHeading,
                    CrosswindKt = components.Crosswind,
                    HeadwindKt = components.Headwind,
                    GustCrosswindKt = components.GustCrosswind,
                    GustHeadwindKt = components.GustHeadwind,
                    AbsoluteCrosswindKt = Math.Abs(components.Crosswind),
                    HasHeadwind = components.Headwind > 0
                });
            }
        }

        // Sort by absolute crosswind (ascending) then by headwind (descending) to recommend best runway
        var sortedRunways = runwayCrosswinds
            .OrderBy(r => r.AbsoluteCrosswindKt)
            .ThenByDescending(r => r.HeadwindKt)
            .ToList();

        // Recommend the runway with lowest crosswind that has a headwind (or lowest crosswind if all tailwinds)
        var recommended = sortedRunways
            .FirstOrDefault(r => r.HasHeadwind)?.RunwayEndId
            ?? sortedRunways.FirstOrDefault()?.RunwayEndId;

        return new AirportCrosswindResponseDto
        {
            AirportIdentifier = airport.IcaoId ?? airport.ArptId ?? icaoCodeOrIdent,
            WindDirectionDegrees = windDirection,
            WindSpeedKt = metar.WindSpeedKt.Value,
            WindGustKt = metar.WindGustKt,
            IsVariableWind = isVariableWind,
            RawMetar = metar.RawText,
            ObservationTime = metar.ObservationTime,
            Runways = sortedRunways,
            RecommendedRunway = recommended
        };
    }

    public CrosswindCalculationResponseDto CalculateCrosswind(CrosswindCalculationRequestDto request)
    {
        _logger.LogInformation(
            "Calculating crosswind for wind {WindDir}@{WindSpeed} runway {RunwayHeading}",
            request.WindDirectionDegrees,
            request.WindSpeedKt,
            request.RunwayHeadingDegrees);

        bool isVariableWind = request.WindDirectionDegrees == null;

        var components = CalculateCrosswindComponents(
            request.WindDirectionDegrees,
            request.WindSpeedKt,
            request.WindGustKt,
            request.RunwayHeadingDegrees,
            isVariableWind);

        return new CrosswindCalculationResponseDto
        {
            CrosswindKt = components.Crosswind,
            HeadwindKt = components.Headwind,
            GustCrosswindKt = components.GustCrosswind,
            GustHeadwindKt = components.GustHeadwind,
            WindDirectionDegrees = request.WindDirectionDegrees,
            WindSpeedKt = request.WindSpeedKt,
            WindGustKt = request.WindGustKt,
            RunwayHeadingDegrees = request.RunwayHeadingDegrees,
            IsVariableWind = isVariableWind
        };
    }

    public async Task<DensityAltitudeResponseDto> GetDensityAltitudeForAirportAsync(
        string icaoCodeOrIdent,
        AirportDensityAltitudeRequestDto? request = null)
    {
        _logger.LogInformation("Calculating density altitude for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);

        // Get airport data
        var airport = await _context.Airports
            .FirstOrDefaultAsync(a =>
                a.IcaoId == icaoCodeOrIdent.ToUpper() ||
                a.ArptId == icaoCodeOrIdent.ToUpper());

        if (airport == null)
        {
            throw new ResourceNotFoundException(
                $"Airport not found for ICAO code or identifier: {icaoCodeOrIdent}");
        }

        if (airport.Elev == null)
        {
            throw new InvalidDataException("Airport missing elevation data");
        }

        // Get METAR data
        var metar = await _metarService.GetMetarForAirport(icaoCodeOrIdent);

        // Get temperature (with optional override)
        double temperatureCelsius;
        if (request?.TemperatureCelsiusOverride != null)
        {
            temperatureCelsius = request.TemperatureCelsiusOverride.Value;
        }
        else if (metar.TempC != null)
        {
            temperatureCelsius = metar.TempC.Value;
        }
        else
        {
            throw new InvalidDataException("METAR missing temperature data and no override provided");
        }

        // Get altimeter (with optional override)
        double altimeterInHg;
        if (request?.AltimeterInHgOverride != null)
        {
            altimeterInHg = request.AltimeterInHgOverride.Value;
        }
        else if (metar.AltimInHg != null)
        {
            altimeterInHg = metar.AltimInHg.Value;
        }
        else
        {
            throw new InvalidDataException("METAR missing altimeter data and no override provided");
        }

        var result = CalculateDensityAltitudeInternal(
            (double)airport.Elev.Value,
            altimeterInHg,
            temperatureCelsius);

        return new DensityAltitudeResponseDto
        {
            AirportIdentifier = airport.IcaoId ?? airport.ArptId ?? icaoCodeOrIdent,
            FieldElevationFt = (double)airport.Elev.Value,
            PressureAltitudeFt = result.PressureAltitude,
            DensityAltitudeFt = result.DensityAltitude,
            IsaTemperatureCelsius = result.IsaTemperature,
            ActualTemperatureCelsius = temperatureCelsius,
            TemperatureDeviationCelsius = result.TemperatureDeviation,
            AltimeterInHg = altimeterInHg,
            RawMetar = metar.RawText,
            ObservationTime = metar.ObservationTime
        };
    }

    public DensityAltitudeResponseDto CalculateDensityAltitude(DensityAltitudeRequestDto request)
    {
        _logger.LogInformation(
            "Calculating density altitude for elevation {Elev}ft, altimeter {Altim}inHg, temp {Temp}C",
            request.FieldElevationFt,
            request.AltimeterInHg,
            request.TemperatureCelsius);

        var result = CalculateDensityAltitudeInternal(
            request.FieldElevationFt,
            request.AltimeterInHg,
            request.TemperatureCelsius);

        return new DensityAltitudeResponseDto
        {
            FieldElevationFt = request.FieldElevationFt,
            PressureAltitudeFt = result.PressureAltitude,
            DensityAltitudeFt = result.DensityAltitude,
            IsaTemperatureCelsius = result.IsaTemperature,
            ActualTemperatureCelsius = request.TemperatureCelsius,
            TemperatureDeviationCelsius = result.TemperatureDeviation,
            AltimeterInHg = request.AltimeterInHg
        };
    }

    private static int ConvertTrueToMagnetic(int trueHeading, decimal? magVarn, string? magHemis)
    {
        if (magVarn == null)
        {
            return trueHeading;
        }

        int variation = (int)magVarn.Value;

        // East variation: subtract from true to get magnetic
        // West variation: add to true to get magnetic
        int magneticHeading = magHemis?.ToUpper() == "E"
            ? trueHeading - variation
            : trueHeading + variation;

        // Normalize to 0-360
        while (magneticHeading <= 0)
            magneticHeading += 360;
        while (magneticHeading > 360)
            magneticHeading -= 360;

        return magneticHeading;
    }

    private static (double Crosswind, double Headwind, double? GustCrosswind, double? GustHeadwind)
        CalculateCrosswindComponents(int? windDirection, int windSpeed, int? gustSpeed, int runwayHeading, bool isVariableWind)
    {
        // For calm winds, all components are zero
        if (windSpeed == 0)
        {
            return (0, 0, null, null);
        }

        // For variable winds, assume worst case (full crosswind)
        if (isVariableWind)
        {
            double? gustCrosswind = gustSpeed.HasValue ? (double)gustSpeed.Value : null;
            return (windSpeed, 0, gustCrosswind, 0);
        }

        // Calculate wind angle relative to runway
        int windAngle = windDirection!.Value - runwayHeading;

        // Normalize to -180 to +180
        while (windAngle > 180)
            windAngle -= 360;
        while (windAngle < -180)
            windAngle += 360;

        // Convert to radians
        double windAngleRadians = windAngle * Math.PI / 180.0;

        // Calculate components
        // Crosswind: positive = from the right, negative = from the left
        double crosswind = Math.Round(windSpeed * Math.Sin(windAngleRadians), 1);
        // Headwind: positive = headwind, negative = tailwind
        double headwind = Math.Round(windSpeed * Math.Cos(windAngleRadians), 1);

        // Calculate gust components if applicable
        double? gustCrosswindResult = null;
        double? gustHeadwindResult = null;
        if (gustSpeed.HasValue)
        {
            gustCrosswindResult = Math.Round(gustSpeed.Value * Math.Sin(windAngleRadians), 1);
            gustHeadwindResult = Math.Round(gustSpeed.Value * Math.Cos(windAngleRadians), 1);
        }

        return (crosswind, headwind, gustCrosswindResult, gustHeadwindResult);
    }

    private static (double PressureAltitude, double DensityAltitude, double IsaTemperature, double TemperatureDeviation)
        CalculateDensityAltitudeInternal(double fieldElevationFt, double altimeterInHg, double temperatureCelsius)
    {
        // Calculate pressure altitude
        // PA = Field Elevation + ((29.92 - Altimeter) * 1000)
        double pressureAltitude = fieldElevationFt + ((StandardPressureInHg - altimeterInHg) * 1000);

        // Calculate ISA (standard) temperature at this pressure altitude
        // ISA Temp = 15 - (PA / 1000) * 2
        double isaTemperature = SeaLevelStandardTempCelsius - (pressureAltitude / 1000) * LapseRateCelsiusPerThousandFt;

        // Calculate temperature deviation from standard
        double temperatureDeviation = temperatureCelsius - isaTemperature;

        // Calculate density altitude
        // DA = PA + (120 * Temperature Deviation)
        double densityAltitude = pressureAltitude + (DensityAltitudeFactor * temperatureDeviation);

        return (
            Math.Round(pressureAltitude, 0),
            Math.Round(densityAltitude, 0),
            Math.Round(isaTemperature, 1),
            Math.Round(temperatureDeviation, 1)
        );
    }
}
