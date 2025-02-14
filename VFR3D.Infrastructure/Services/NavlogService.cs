using GeographicLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class NavlogService : INavlogService
{
    private readonly VFR3DDbContext _context;
    private readonly IWindsAloftService _windsAloftService;
    private readonly IMagneticVariationService _magneticVariationService;
    private readonly ILogger<NavlogService> _logger;

    public NavlogService(
        VFR3DDbContext context,
        IWindsAloftService windsAloftService,
        IMagneticVariationService magneticVariationService,
        ILogger<NavlogService> logger)
    {
        _context = context;
        _windsAloftService = windsAloftService;
        _magneticVariationService = magneticVariationService;
        _logger = logger;
    }

    public async Task<NavlogResponseDto> CalculateNavlog(NavlogRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting navlog calculation for {WaypointCount} waypoints",
                request.Waypoints.Count);

            if (request.Waypoints.Count < 2)
            {
                throw new ArgumentException("At least two waypoints are required for navigation");
            }

            var performanceProfile = await _context.AircraftPerformanceProfiles
                                         .FirstOrDefaultAsync(p => p.Id == request.AircraftPerformanceProfileId)
                                     ?? throw new KeyNotFoundException(
                                         $"Aircraft performance profile not found: {request.AircraftPerformanceProfileId}");

            var waypointsWithClimbAndDescent = AddClimbAndDescentWaypoints(
                request.Waypoints,
                request.PlannedCruisingAltitude,
                performanceProfile);

            var response = new NavlogResponseDto
            {
                TotalRouteDistance = 0,
                TotalRouteTimeHours = 0,
                TotalFuelUsed = 0,
                AverageWindComponent = 0,
                Legs = []
            };

            // Determine the forecast type and get winds aloft data
            var (forecastType, windsAloftData) = await DetermineForecastType(request.TimeOfDeparture);
            if (!forecastType.HasValue || windsAloftData == null)
            {
                _logger.LogWarning("No suitable winds aloft forecast found for departure time");
            }

            var previousLegEndTime = request.TimeOfDeparture;

            for (var i = 0; i < waypointsWithClimbAndDescent.Count - 1; i++)
            {
                var isClimbLeg = i == 0;
                var isDescentLeg = i == waypointsWithClimbAndDescent.Count - 2;

                var leg = await ProcessLeg(
                    isClimbLeg,
                    isDescentLeg,
                    waypointsWithClimbAndDescent[i],
                    waypointsWithClimbAndDescent[i + 1],
                    performanceProfile,
                    previousLegEndTime,
                    windsAloftData);

                response.Legs.Add(leg);
                previousLegEndTime = leg.EndLegTime;
            }

            response.TotalRouteDistance = CalculateTotalRouteDistance(response.Legs);
            response.TotalFuelUsed = CalculateTotalFuelUsed(response.Legs, performanceProfile.SttFuelGals);
            response.TotalRouteTimeHours = CalculateTotalRouteTime(response.Legs);
            response.AverageWindComponent = CalculateAverageHeadwind(response.Legs);

            CalculateDistanceRemaining(response.Legs, response.TotalRouteDistance);
            CalculateRemainingFuel(response.Legs, performanceProfile.FuelOnBoardGals);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating navlog");
            throw;
        }
    }

    public async Task<BearingAndDistanceResponseDto> CalculateBearingAndDistance(BearingAndDistanceRequestDto request)
    {
        var inverseGeodesicResult = CalculateInverseGeodesic(request.StartLatitude, request.StartLongitude, request.EndLatitude,
            request.EndLongitude);
        
        if(inverseGeodesicResult == null) return new BearingAndDistanceResponseDto();
        var magneticCourse = await CalculateMagneticCourse(request.StartLatitude, request.StartLongitude, inverseGeodesicResult.Azimuth2);

        return new BearingAndDistanceResponseDto
        {
            Distance = inverseGeodesicResult.Distance / Constants.NauticalMile,
            TrueCourse = inverseGeodesicResult.Azimuth2,
            MagneticCourse = magneticCourse
        };
    }


    public async Task<WindsAloftDto> GetWindsAloftData(int forecast)
    {
        return await _windsAloftService.FetchWindsAloftData(forecast);
    }

    private List<WaypointDto> AddClimbAndDescentWaypoints(
        List<WaypointDto> waypoints,
        int plannedCruisingAltitude,
        AircraftPerformanceProfile performance)
    {
        var startPoint = waypoints[0];
        var endPoint = waypoints[^1];

        // Calculate climb point
        var altitudeDifference = plannedCruisingAltitude - startPoint.Altitude;
        var climbTime = altitudeDifference / (performance.ClimbFpm * 60); // convert to hours
        var climbDistance = performance.ClimbTrueAirspeed * climbTime; // Nautical Miles

        var topOfClimbPoint = FindPointAtDistance(
            startPoint,
            climbDistance,
            CalculateTrueCourse(startPoint.Latitude, startPoint.Longitude, waypoints[1].Latitude, waypoints[1].Longitude));
        
        var topOfClimbWaypoint = new WaypointDto
        {
            Id = "TOC",
            Name = "TOC",
            Latitude = topOfClimbPoint.Latitude,
            Longitude = topOfClimbPoint.Longitude,
            Altitude = plannedCruisingAltitude,
            WaypointType = WaypointType.CalculatedPoint
        };

        // Calculate descent point
        var descentAltitudeDifference = plannedCruisingAltitude - endPoint.Altitude;
        var descentTime = descentAltitudeDifference / (performance.DescentFpm * 60); // convert to hours
        var descentDistance = performance.DescentTrueAirspeed * descentTime; // Nautical Miles

        var topOfDescentPoint = FindPointAtDistance(
            endPoint,
            -descentDistance,
            CalculateTrueCourse(waypoints[^2].Latitude, waypoints[^2].Longitude, endPoint.Latitude, endPoint.Longitude));

        var topOfDescentWaypoint = new WaypointDto
        {
            Id = "TOD",
            Name = "TOD",
            Latitude = topOfDescentPoint.Latitude,
            Longitude = topOfDescentPoint.Longitude,
            Altitude = plannedCruisingAltitude,
            WaypointType = WaypointType.CalculatedPoint
        };

        var result = new List<WaypointDto>();
        result.Add(startPoint);
        result.Add(topOfClimbWaypoint);
        result.AddRange(waypoints.Skip(1).Take(waypoints.Count - 2));
        result.Add(topOfDescentWaypoint);
        result.Add(endPoint);

        return result;
    }

    private async Task<NavigationLegDto> ProcessLeg(
        bool isClimbLeg,
        bool isDescentLeg,
        WaypointDto startPoint,
        WaypointDto endPoint,
        AircraftPerformanceProfile performance,
        DateTime previousLegEndTime,
        WindsAloftDto? windsAloftData)
    {
        var inverseGeodesicResult = CalculateInverseGeodesic(startPoint.Latitude, startPoint.Longitude, endPoint.Latitude, endPoint.Longitude);
        var leg = new NavigationLegDto
        {
            LegStartPoint = startPoint,
            LegEndPoint = endPoint,
            StartLegTime = previousLegEndTime,
        };

        if (inverseGeodesicResult == null) return leg;
        var magneticCourse = await CalculateMagneticCourse(startPoint.Latitude, startPoint.Longitude, inverseGeodesicResult.Azimuth2);

        leg = new NavigationLegDto()
        {
            LegStartPoint = startPoint,
            LegEndPoint = endPoint,
            TrueCourse = inverseGeodesicResult.Azimuth2,
            MagneticCourse = magneticCourse,
            LegDistance = inverseGeodesicResult.Distance / Constants.NauticalMile,
            StartLegTime = previousLegEndTime
        };

        var legTas = GetLegTas(isClimbLeg, isDescentLeg, performance);
        var windTempData = DetermineWindsAloftForWaypoint(startPoint, windsAloftData);

        ApplyWindAndTemperatureData(leg, windTempData, legTas);
        CalculateLegTimeAndFuel(leg, isClimbLeg, isDescentLeg, performance);

        return leg;
    }

    private double CalculateTrueCourse(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
    {
        var result = Geodesic.WGS84.Inverse(
            startLatitude, 
            startLongitude,
            endLatitude, 
            endLongitude);

        // Normalize to 0-360
        var course = result.Azimuth2;
        while (course < 0) course += 360;
        while (course >= 360) course -= 360;

        return course;
    }

    private async Task<double> CalculateMagneticCourse(double latitude, double longitude, double trueCourse)
    {
        var magneticVariation = await _magneticVariationService.GetMagneticVariation(
            latitude, 
            longitude);
        // West headings come out negative, so we need to (subtract, which would come out to adding) it to our true course.
        // Easterly headings come out positive, so they get subtracted from our true course.
        var magneticCourse = trueCourse - magneticVariation;

        // Normalize to 0-360
        while (magneticCourse < 0) magneticCourse += 360;
        while (magneticCourse >= 360) magneticCourse -= 360;

        return magneticCourse;
    }
    
    private InverseGeodesicResult? CalculateInverseGeodesic(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
    {
        var result = Geodesic.WGS84.Inverse(
            startLatitude, 
            startLongitude,
            endLatitude, 
            endLongitude);

        if (result == null)
        {
            _logger.LogWarning("Inverse geodesic calculation result was null. Bearing and distance calculations will not be accurate.");
        }

        return result;
    }

    private int GetLegTas(bool isClimbLeg, bool isDescentLeg, AircraftPerformanceProfile performance)
    {
        if (isClimbLeg) return performance.ClimbTrueAirspeed;
        if (isDescentLeg) return performance.DescentTrueAirspeed;
        return performance.CruiseTrueAirspeed;
    }

    private void ApplyWindAndTemperatureData(
        NavigationLegDto leg,
        WindTempDto? windTempData,
        int legTas)
    {
        if (windTempData != null)
        {
            leg.WindDir = windTempData.Direction ?? 0;
            leg.WindSpeed = windTempData.Speed;
            leg.TempC =  windTempData.Temperature ?? 0;

            if (leg.WindDir != 0 && leg.WindSpeed != 0)
            {
                leg.MagneticHeading = CalculateMagneticHeading(leg, legTas, windTempData);
                leg.GroundSpeed = CalculateGroundSpeed(legTas, windTempData, leg.TrueCourse);
            }
            else
            {
                leg.MagneticHeading = leg.MagneticCourse;
                leg.GroundSpeed = legTas;
            }
        }
        else
        {
            leg.MagneticHeading = leg.MagneticCourse;
            leg.GroundSpeed = legTas;
        }
    }

    private double CalculateMagneticHeading(
        NavigationLegDto leg,
        int legTas,
        WindTempDto windTempData)
    {
        var windCorrectionAngle = CalculateWindCorrectionAngle(
            leg.TrueCourse,
            windTempData.Direction ?? 0,
            windTempData.Speed,
            legTas,
            leg);

        var magneticHeading = leg.MagneticCourse + windCorrectionAngle;

        // Normalize to 0-360
        while (magneticHeading < 0) magneticHeading += 360;
        while (magneticHeading >= 360) magneticHeading -= 360;

        return magneticHeading;
    }

    private double CalculateGroundSpeed(int legTrueAirSpeed, WindTempDto windTempData, double trueCourse)
    {
        const double radiansPerDegree = Math.PI / 180.0;
        var relativeWind = Math.Abs(trueCourse - ((windTempData.Direction ?? 0) + 180) % 360);
        var headwindComponent = windTempData.Speed * Math.Cos(relativeWind * radiansPerDegree);
        return legTrueAirSpeed + headwindComponent;
    }

    private double CalculateWindCorrectionAngle(
        double trueCourse,
        int windDirection,
        int windSpeed,
        int indicatedAirspeed,
        NavigationLegDto leg)
    {
        const double radiansPerDegree = Math.PI / 180.0;
        var relativeWindDirection = windDirection - (double)trueCourse;
        var crosswindComponent = windSpeed * Math.Sin(relativeWindDirection * radiansPerDegree);
        var headwindComponent = windSpeed * Math.Cos(relativeWindDirection * radiansPerDegree);

        leg.HeadwindComponent = headwindComponent;

        var windCorrectionAngle = Math.Atan(crosswindComponent / (indicatedAirspeed - headwindComponent));
        return windCorrectionAngle / radiansPerDegree;
    }
    
    private void CalculateLegTimeAndFuel(
            NavigationLegDto leg,
            bool isClimbLeg,
            bool isDescentLeg,
            AircraftPerformanceProfile performance)
        {
            if (leg.GroundSpeed > 0)
            {
                var legTimeHours = leg.LegDistance / leg.GroundSpeed;
                leg.EndLegTime = leg.StartLegTime.AddHours((double)legTimeHours);

                leg.LegFuelBurnGals = isClimbLeg
                    ? performance.ClimbFuelBurn * legTimeHours
                    : isDescentLeg
                        ? performance.DescentFuelBurn * legTimeHours
                        : performance.CruiseFuelBurn * legTimeHours;
            }
            else
            {
                leg.EndLegTime = leg.StartLegTime;
                leg.LegFuelBurnGals = 0;
            }
        }

        private double CalculateTotalRouteDistance(List<NavigationLegDto> legs)
        {
            return legs.Sum(leg => leg.LegDistance);
        }

        private double CalculateTotalFuelUsed(List<NavigationLegDto> legs, double sttFuel)
        {
            return sttFuel + legs.Sum(leg => leg.LegFuelBurnGals);
        }

        private double CalculateTotalRouteTime(List<NavigationLegDto> legs)
        {
            if (!legs.Any()) return 0;

            var totalMinutes = (legs[^1].EndLegTime - legs[0].StartLegTime).TotalMinutes;
            return totalMinutes / 60.0; // Convert to hours
        }

        private void CalculateDistanceRemaining(List<NavigationLegDto> legs, double totalRouteDistance)
        {
            double cumulativeDistance = 0;
            foreach (var leg in legs)
            {
                cumulativeDistance += leg.LegDistance;
                leg.DistanceRemaining = totalRouteDistance - cumulativeDistance;
            }
        }

        private void CalculateRemainingFuel(List<NavigationLegDto> legs, double startingFuel)
        {
            double remainingFuel = startingFuel;
            foreach (var leg in legs)
            {
                remainingFuel -= leg.LegFuelBurnGals;
                leg.RemainingFuelGals = remainingFuel;
            }
        }

        private double CalculateAverageHeadwind(List<NavigationLegDto> legs)
        {
            if (legs.Count == 0) return 0;
            return legs.Average(leg => leg.HeadwindComponent);
        }

        private WaypointDto FindPointAtDistance(WaypointDto startPoint, double distanceNauticalMiles, double trueCourse)
        {
            var distanceMeters = (distanceNauticalMiles * Constants.NauticalMile);
            var result = Geodesic.WGS84.Direct(
                startPoint.Latitude,
                startPoint.Longitude,
                trueCourse,
                distanceMeters);

            return new WaypointDto
            {
                Latitude = result.Latitude,
                Longitude = result.Longitude,
                Altitude = startPoint.Altitude,
                WaypointType = WaypointType.CalculatedPoint
            };
        }

        private async Task<(int? ForecastType, WindsAloftDto? WindsAloftData)> DetermineForecastType(DateTime departureTime)
        {
            int[] forecastTypes = { 6, 12, 24 };

            foreach (var forecastType in forecastTypes)
            {
                try
                {
                    var windsAloftData = await _windsAloftService.FetchWindsAloftData(forecastType);

                    if (departureTime >= windsAloftData.ForUseStartTime && 
                        departureTime < windsAloftData.ForUseEndTime)
                    {
                        return (forecastType, windsAloftData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching winds aloft data for forecast type {ForecastType}", forecastType);
                }
            }

            _logger.LogWarning("No suitable forecast found for departure time {DepartureTime}", departureTime);
            return (null, null);
        }

        private WindTempDto? DetermineWindsAloftForWaypoint(
            WaypointDto waypoint,
            WindsAloftDto? windsAloftData)
        {
            if (windsAloftData == null) return null;

            var nearestAirport = FindNearestWindsAloftAirport(waypoint, windsAloftData);
            if (nearestAirport == null) return null;

            var altitude = (int)waypoint.Altitude;
            if (nearestAirport.WindTemp.ContainsKey(altitude.ToString()))
            {
                return nearestAirport.WindTemp[altitude.ToString()];
            }

            return InterpolateWindTempData(altitude, nearestAirport);
        }

        private WindsAloftSiteDto? FindNearestWindsAloftAirport(WaypointDto waypoint, WindsAloftDto windsAloftData)
        {
            if (!string.IsNullOrEmpty(waypoint.Name))
            {
                // First try to find an exact match by airport code
                var airportCode = waypoint.Name;
                if (airportCode.Length == 4 && airportCode.StartsWith("K"))
                {
                    airportCode = airportCode[1..];
                }

                var matchingAirport = windsAloftData.WindTemp.FirstOrDefault(a => a.Id == airportCode);
                if (matchingAirport != null) return matchingAirport;
            }

            // If no exact match, find nearest airport using geodesic calculations
            var airports = windsAloftData.WindTemp.Select(a =>
            {
                var result = Geodesic.WGS84.Inverse(
                    waypoint.Latitude,
                    waypoint.Longitude,
                    a.Lat,
                    a.Lon);

                return new
                {
                    Airport = a,
                    Distance = result.Distance
                };
            });

            return airports.MinBy(a => a.Distance)?.Airport;
        }

        private WindTempDto? InterpolateWindTempData(int altitude, WindsAloftSiteDto airport)
        {
            var altitudeLevels = new[] { 3000, 6000, 9000, 12000, 18000, 24000, 30000, 34000, 39000 };

            // Find the closest lower and upper altitudes
            var lowerAltIndex = Array.FindIndex(altitudeLevels, a => a > altitude) - 1;
            var upperAltIndex = lowerAltIndex + 1;

            // Handle edge cases
            if (lowerAltIndex < 0)
            {
                return airport.WindTemp.GetValueOrDefault(altitudeLevels[0].ToString());
            }
            if (upperAltIndex >= altitudeLevels.Length)
            {
                return airport.WindTemp.GetValueOrDefault(altitudeLevels[^1].ToString());
            }

            var lowerAlt = altitudeLevels[lowerAltIndex];
            var upperAlt = altitudeLevels[upperAltIndex];

            if (!airport.WindTemp.TryGetValue(lowerAlt.ToString(), out var lowerData) ||
                !airport.WindTemp.TryGetValue(upperAlt.ToString(), out var upperData))
            {
                return null;
            }

            var ratio = (altitude - lowerAlt) / (double)(upperAlt - lowerAlt);

            // Interpolate direction
            int? direction = null;
            if (lowerData.Direction.HasValue && upperData.Direction.HasValue)
            {
                var dirDiff = upperData.Direction.Value - lowerData.Direction.Value;
                if (Math.Abs(dirDiff) > 180)
                {
                    dirDiff = dirDiff > 0 ? dirDiff - 360 : dirDiff + 360;
                }
                direction = (int)((lowerData.Direction.Value + dirDiff * ratio + 360) % 360);
            }

            // Interpolate speed
            var speed = (int)(lowerData.Speed + (upperData.Speed - lowerData.Speed) * ratio);

            // Interpolate temperature
            float? temperature = null;
            if (lowerData.Temperature.HasValue && upperData.Temperature.HasValue)
            {
                temperature = (float)(lowerData.Temperature.Value + 
                    (upperData.Temperature.Value - lowerData.Temperature.Value) * ratio);
            }

            return new WindTempDto()
            {
                Direction = direction,
                Speed = speed,
                Temperature = temperature
            };
        }
    }
