using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services;
using Xunit;

namespace VFR3D.Tests.Services
{
        public class NavlogServiceTests
        {
            private readonly IAircraftPerformanceProfileRepository _aircraftPerformanceProfileRepository;
            private readonly IWindsAloftService _windsAloftService;
            private readonly IMagneticVariationService _magneticVariationService;
            private readonly IAirspaceService _airspaceService;
            private readonly IObstacleService _obstacleService;
            private readonly ILogger<NavlogService> _logger;
            private readonly NavlogService _navlogService;
            private readonly Faker _faker;

            public NavlogServiceTests()
            {
                // Setup mocks
                _aircraftPerformanceProfileRepository = Substitute.For<IAircraftPerformanceProfileRepository>();
                _windsAloftService = Substitute.For<IWindsAloftService>();
                _airspaceService = Substitute.For<IAirspaceService>();
                _obstacleService = Substitute.For<IObstacleService>();
                _magneticVariationService = Substitute.For<IMagneticVariationService>();
                _logger = Substitute.For<ILogger<NavlogService>>();

                // Setup NavlogService
                _navlogService = new NavlogService(
                    _aircraftPerformanceProfileRepository,
                    _windsAloftService,
                    _airspaceService,
                    _obstacleService,
                    _magneticVariationService,
                    _logger
                );

                // Setup test performance profile that will be returned by the repository
                var testProfile = new AircraftPerformanceProfile
                {
                    Id = "test-profile-id",
                    UserId = "test-user-id",
                    ProfileName = "Test Profile",
                    ClimbTrueAirspeed = 90,
                    CruiseTrueAirspeed = 120,
                    DescentTrueAirspeed = 100,
                    ClimbFpm = 500,
                    DescentFpm = 500,
                    ClimbFuelBurn = 8.0,
                    CruiseFuelBurn = 6.5,
                    DescentFuelBurn = 4.0,
                    SttFuelGals = 3.0,
                    FuelOnBoardGals = 40.0
                };

                // Configure the repository to return the test profile when GetByIdAsync is called with "test-profile-id"
                _aircraftPerformanceProfileRepository.GetByIdAsync("test-profile-id").Returns(testProfile);

                // For the test that expects KeyNotFoundException, explicitly return null
                _aircraftPerformanceProfileRepository.GetByIdAsync("non-existent-profile").Returns((AircraftPerformanceProfile)null);

                // Setup Faker for generating test data with a fixed seed for reproducibility
                _faker = new Faker("en") { Random = new Randomizer(12345) };
            }

            [Fact]
            public async Task CalculateNavlog_ShouldThrowArgumentException_WhenWaypointsLessThanTwo()
            {
                // Arrange
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = DateTime.UtcNow,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Act
                Func<Task> act = async () => await _navlogService.CalculateNavlog(request);

                // Assert
                await act.Should().ThrowAsync<ArgumentException>()
                    .WithMessage("At least two waypoints are required for navigation");
            }

            [Fact]
            public async Task CalculateNavlog_ShouldThrowKeyNotFoundException_WhenProfileNotFound()
            {
                // Arrange
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = DateTime.UtcNow,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                        CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "non-existent-profile"
                };

                // Act
                Func<Task> act = async () => await _navlogService.CalculateNavlog(request);

                // Assert
                await act.Should().ThrowAsync<KeyNotFoundException>()
                    .WithMessage("Aircraft performance profile not found: non-existent-profile");
            }

            [Fact]
            public async Task CalculateNavlog_ShouldAddClimbAndDescentWaypoints()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                        CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup the mock for magnetic variation
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(0);

                // Setup mock for winds aloft
                var windsAloftData = GenerateWindsAloftData(departureTime);
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns(windsAloftData);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();
                result.Legs.Should().HaveCount(3); // Start -> TOC -> End with TOD close to End
                result.Legs[0].LegStartPoint.Name.Should().Be("KBNA");
                result.Legs[0].LegEndPoint.Name.Should().Be("TOC");
                result.Legs[1].LegStartPoint.Name.Should().Be("TOC");
                result.Legs[1].LegEndPoint.Name.Should().Be("TOD");
                result.Legs[2].LegStartPoint.Name.Should().Be("TOD");
                result.Legs[2].LegEndPoint.Name.Should().Be("KATL");
            }

            [Fact]
            public async Task CalculateNavlog_ShouldCalculateCorrectTotals()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                        CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup the mock for magnetic variation
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(0);

                // Setup mock for winds aloft
                var windsAloftData = GenerateWindsAloftData(departureTime);
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns(windsAloftData);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();
                result.TotalRouteDistance.Should().BeGreaterThan(0);
                result.TotalRouteTimeHours.Should().BeGreaterThan(0);
                result.TotalFuelUsed.Should().BeGreaterThan(3.0); // At least STT fuel
                result.Legs.Should().HaveCount(3);

                // Check that the last leg has zero remaining distance
                result.Legs.Last().DistanceRemaining.Should().BeApproximately(0, 0.01);

                // Verify that legs have reasonable fuel numbers
                foreach (var leg in result.Legs)
                {
                    leg.LegFuelBurnGals.Should().BeGreaterThan(0);
                    leg.RemainingFuelGals.Should().BeGreaterThan(0);
                    leg.RemainingFuelGals.Should().BeLessThan(40.0); // Starting fuel
                }
            }

            [Theory]
            [InlineData("KBNA", 36.1244, -86.6782, "KATL", 33.6367, -84.4278, 186)]
            [InlineData("KJFK", 40.6413, -73.7781, "KBOS", 42.3656, -71.0096, 162)]
            [InlineData("KSFO", 37.6188, -122.3758, "KLAX", 33.9416, -118.4085, 293)]
            [InlineData("KDEN", 39.8617, -104.6731, "KPHX", 33.4343, -112.0126, 523)]
            [InlineData("KMIA", 25.7932, -80.2906, "KJFK", 40.6413, -73.7781, 947)]
            [InlineData("KSEA", 47.44472, -122.31361, "KPDX", 45.59578, -122.60917, 112)]
            public async Task CalculateBearingAndDistance_ShouldCalculateCorrectDistancesBetweenAirports(
                string startAirport, double startLat, double startLon,
                string endAirport, double endLat, double endLon,
                double expectedDistanceNM)
            {
                // Arrange
                var request = new BearingAndDistanceRequestDto
                {
                    StartLatitude = startLat,
                    StartLongitude = startLon,
                    EndLatitude = endLat,
                    EndLongitude = endLon
                };

                // Setup magnetic variation - simplified for test consistency
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(5.0);

                // Act
                var result = await _navlogService.CalculateBearingAndDistance(request);

                // Assert
                result.Should().NotBeNull();

                // Log for debugging
                _logger.LogInformation("Distance from {Start} to {End}: Expected {ExpectedDist} NM, Actual {ActualDist} NM",
                    startAirport, endAirport, expectedDistanceNM, result.Distance);

                // Verify distance - 2% tolerance
                double tolerance = expectedDistanceNM * 0.02;
                result.Distance.Should().BeApproximately(expectedDistanceNM, tolerance);

                // Verify magnetic variation is applied correctly
                result.MagneticCourse.Should().BeApproximately(result.TrueCourse - 5.0, 0.1);
            }

            [Fact]
            public async Task GetWindsAloftData_ShouldReturnDataFromService()
            {
                // Arrange
                var forecast = 6;
                var expectedData = new WindsAloftDto
                {
                    ValidTime = DateTime.UtcNow,
                    ForUseStartTime = DateTime.UtcNow.AddHours(-3),
                    ForUseEndTime = DateTime.UtcNow.AddHours(3),
                    WindTemp = new List<WindsAloftSiteDto>()
                };

                _windsAloftService.FetchWindsAloftData(forecast)
                    .Returns(expectedData);

                // Act
                var result = await _navlogService.GetWindsAloftData(forecast);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(expectedData);

                // Verify the service was called
                await _windsAloftService.Received(1).FetchWindsAloftData(forecast);
            }

            [Theory]
            [InlineData(30.0, -85.0, 35.0, -90.0)] // Florida to Tennessee
            [InlineData(47.0, -122.0, 34.0, -118.0)] // Seattle to Los Angeles
            [InlineData(40.7, -74.0, 41.9, -87.6)] // New York to Chicago
            [InlineData(25.8, -80.3, 49.0, -123.0)] // Miami to Vancouver (long route)
            public async Task CalculateBearingAndDistance_ShouldHandleVariousRoutes(double startLat, double startLon, double endLat, double endLon)
            {
                // Arrange
                var request = new BearingAndDistanceRequestDto
                {
                    StartLatitude = startLat,
                    StartLongitude = startLon,
                    EndLatitude = endLat,
                    EndLongitude = endLon
                };

                // Setup consistent magnetic variation for test simplicity
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(5.0);

                // Act
                var result = await _navlogService.CalculateBearingAndDistance(request);

                // Assert
                result.Should().NotBeNull();
                result.Distance.Should().BeGreaterThan(0);
                result.TrueCourse.Should().BeGreaterThanOrEqualTo(0);
                result.TrueCourse.Should().BeLessThan(360);
                result.MagneticCourse.Should().BeGreaterThanOrEqualTo(0);
                result.MagneticCourse.Should().BeLessThan(360);
            }

            [Fact]
            public async Task CalculateNavlog_ShouldHandleMultiWaypointRoute()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                        CreateSpecificWaypoint("KMEM", 35.0420, -89.9768, 341),
                        CreateSpecificWaypoint("KLIT", 34.7294, -92.2243, 266),
                        CreateSpecificWaypoint("KDFW", 32.8968, -97.0380, 607)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup the mock for magnetic variation
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(0);

                // Setup mock for winds aloft
                var windsAloftData = GenerateWindsAloftData(departureTime);
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns(windsAloftData);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();
                // TOC + (n-2) waypoints + TOD + Final = n+1 legs where n is the original waypoint count
                result.Legs.Should().HaveCount(5);

                // Check that all legs have correct endpoints
                result.Legs[0].LegStartPoint.Name.Should().Be("KBNA");
                result.Legs[0].LegEndPoint.Name.Should().Be("TOC");
                result.Legs[1].LegStartPoint.Name.Should().Be("TOC");
                result.Legs[1].LegEndPoint.Name.Should().Be("KMEM");
                result.Legs[2].LegStartPoint.Name.Should().Be("KMEM");
                result.Legs[2].LegEndPoint.Name.Should().Be("KLIT");
                result.Legs[3].LegStartPoint.Name.Should().Be("KLIT");
                result.Legs[3].LegEndPoint.Name.Should().Be("TOD");
                result.Legs[4].LegStartPoint.Name.Should().Be("TOD");
                result.Legs[4].LegEndPoint.Name.Should().Be("KDFW");
            }

            [Fact]
            public async Task CalculateNavlog_ShouldHandleNoWindsData()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                        CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
                    },
                    PlannedCruisingAltitude = 7500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup the mock for magnetic variation
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(0);

                // Return null from FetchWindsAloftData to simulate no winds data
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns((WindsAloftDto?)null);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();

                // In no-wind conditions, ground speed should equal true airspeed
                result.Legs[1].GroundSpeed.Should().Be(120); // Cruise TAS

                // And heading should match course
                result.Legs[1].MagneticHeading.Should().Be(result.Legs[1].MagneticCourse);
            }

            [Fact]
            public async Task CalculateNavlog_ShouldHandleExtremeMagneticVariation()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("PAFA", 64.80389, -147.87611, 439), // Fairbanks, Alaska
                        CreateSpecificWaypoint("PANC", 61.17444, -149.99611, 152)  // Anchorage, Alaska
                    },
                    PlannedCruisingAltitude = 9500,
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup extreme magnetic variation (common near the poles)
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(20.0); // 20 degrees East variation

                // Setup mock for winds aloft
                var windsAloftData = GenerateWindsAloftData(departureTime);
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns(windsAloftData);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();

                // Verify that magnetic course = true course - magnetic variation
                result.Legs[1].MagneticCourse.Should().BeApproximately(result.Legs[1].TrueCourse - 20.0, 1.0);
            }

            [Fact]
            public async Task CalculateNavlog_ShouldHandleHighElevationAirports()
            {
                // Arrange
                var departureTime = DateTime.UtcNow;
                var request = new NavlogRequestDto
                {
                    TimeOfDeparture = departureTime,
                    Waypoints = new List<WaypointDto> {
                        CreateSpecificWaypoint("KEGE", 39.6426, -106.9127, 6548), // Eagle County, CO (high elevation)
                        CreateSpecificWaypoint("KASE", 39.2232, -106.8685, 7820)  // Aspen, CO (very high elevation)
                    },
                    PlannedCruisingAltitude = 13500, // Higher cruise altitude for mountains
                    AircraftPerformanceProfileId = "test-profile-id"
                };

                // Setup the mock for magnetic variation
                _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>())
                    .Returns(10.0);

                // Setup mock for winds aloft
                var windsAloftData = GenerateWindsAloftData(departureTime);
                _windsAloftService.FetchWindsAloftData(Arg.Any<int>())
                    .Returns(windsAloftData);

                // Act
                var result = await _navlogService.CalculateNavlog(request);

                // Assert
                result.Should().NotBeNull();
                result.Legs.Should().HaveCount(3);

                // Check climb and descent calculations with high elevation airports
                result.Legs[0].LegStartPoint.Name.Should().Be("KEGE");
                result.Legs[0].LegStartPoint.Altitude.Should().Be(6548);
                result.Legs[0].LegEndPoint.Name.Should().Be("TOC");
                result.Legs[0].LegEndPoint.Altitude.Should().Be(13500);

                result.Legs[2].LegStartPoint.Name.Should().Be("TOD");
                result.Legs[2].LegStartPoint.Altitude.Should().Be(13500);
                result.Legs[2].LegEndPoint.Name.Should().Be("KASE");
                result.Legs[2].LegEndPoint.Altitude.Should().Be(7820);
            }

        [Theory]
        [InlineData("KBNA", 36.1244, -86.6782, "KATL", 33.6367, -84.4278, 215, 9, 118)]
        [InlineData("KJFK", 40.6413, -73.7781, "KBOS", 42.3656, -71.0096, 210, 10, 135)]
        [InlineData("KSFO", 37.6188, -122.3758, "KLAX", 33.9416, -118.4085, 210, 10, 116)]
        [InlineData("KDEN", 39.8617, -104.6731, "KPHX", 33.4343, -112.0126, 210, 10, 105)]
        [InlineData("KMIA", 25.7932, -80.2906, "KJFK", 40.6413, -73.7781, 210, 10, 134)]
        public async Task CalculateNavlog_ShouldCalculateCorrectGroundspeedWithDifferentWinds(
            string startAirport, double startLat, double startLon,
            string endAirport, double endLat, double endLon,
            int windDir, int windSpeed, int expectedGroundSpeed)
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = new List<WaypointDto>
                {
                    CreateSpecificWaypoint(startAirport, startLat, startLon, 500),
                    CreateSpecificWaypoint(endAirport, endLat, endLon, 500)
                },
                PlannedCruisingAltitude = 6000,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            // Setup winds aloft with controlled parameters
            var windsAloftData = GenerateWindsAloftData(departureTime, windDir, windSpeed);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Setup zero magnetic variation for simpler testing
            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            result.Should().NotBeNull();
            result.Legs[1].GroundSpeed.Should().BeApproximately(expectedGroundSpeed, 5); // Allow small margin
        }

        [Fact]
        public async Task CalculateNavlog_ShouldInsertTOCAndTOD_PerSegment_WithRefuelStops()
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var refuelAltitude = 700;
            var waypoints = new List<WaypointDto>
            {
                CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                new WaypointDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "KXYZ",
                    Latitude = 35.5,
                    Longitude = -86.0,
                    Altitude = refuelAltitude,
                    WaypointType = WaypointType.Airport,
                    IsRefuelingStop = true,
                    RefuelToFull = true
                },
                CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
            };

            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = waypoints,
                PlannedCruisingAltitude = 7500,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);
            var windsAloftData = GenerateWindsAloftData(departureTime);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            result.Should().NotBeNull();
            // Start, TOC-1, TOD-1, Refuel, TOC-2, TOD-2, End => 7 waypoints => 6 legs
            result.Legs.Should().HaveCount(6);

            result.Legs.Select(l => l.LegEndPoint.Id).Where(id => id.StartsWith("TOC-")).Distinct().Count().Should().Be(2);
            result.Legs.Select(l => l.LegEndPoint.Id).Where(id => id.StartsWith("TOD-")).Distinct().Count().Should().Be(2);
            result.Legs.Any(l => l.LegEndPoint.Name == "TOC").Should().BeTrue();
            result.Legs.Any(l => l.LegEndPoint.Name == "TOD").Should().BeTrue();
        }

        [Fact]
        public async Task CalculateNavlog_ShouldDeductSTT_AtStart_And_AfterRefuel_Full()
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var waypoints = new List<WaypointDto>
            {
                CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                new WaypointDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "KXYZ",
                    Latitude = 35.5,
                    Longitude = -86.0,
                    Altitude = 700,
                    WaypointType = WaypointType.Airport,
                    IsRefuelingStop = true,
                    RefuelToFull = true
                },
                CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
            };

            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = waypoints,
                PlannedCruisingAltitude = 7500,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);
            var windsAloftData = GenerateWindsAloftData(departureTime);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            var stt = 3.0; // from test profile
            var full = 40.0; // from test profile

            // First leg should reflect STT deduction at start
            var leg0 = result.Legs[0];
            var expectedAfterLeg0 = full - stt - leg0.LegFuelBurnGals;
            leg0.RemainingFuelGals.Should().BeApproximately(expectedAfterLeg0, 0.1);

            // Leg ending at refuel should show full fuel immediately after refuel
            var legEndingAtRefuel = result.Legs.First(l => l.LegEndPoint.Name == "KXYZ");
            legEndingAtRefuel.RemainingFuelGals.Should().BeApproximately(full, 0.1);

            // Next leg (after refuel) should apply STT again
            var legAfterRefuel = result.Legs[result.Legs.IndexOf(legEndingAtRefuel) + 1];
            var expectedAfterNext = full - stt - legAfterRefuel.LegFuelBurnGals;
            legAfterRefuel.RemainingFuelGals.Should().BeApproximately(expectedAfterNext, 0.1);
        }

        [Fact]
        public async Task CalculateNavlog_ShouldCapRefuelGallons_AtFull()
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var waypoints = new List<WaypointDto>
            {
                CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                new WaypointDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "KXYZ",
                    Latitude = 35.5,
                    Longitude = -86.0,
                    Altitude = 700,
                    WaypointType = WaypointType.Airport,
                    IsRefuelingStop = true,
                    RefuelToFull = false,
                    RefuelGallons = 100 // exceed capacity
                },
                CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
            };

            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = waypoints,
                PlannedCruisingAltitude = 7500,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);
            var windsAloftData = GenerateWindsAloftData(departureTime);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            var full = 40.0; // from test profile
            var legEndingAtRefuel = result.Legs.First(l => l.LegEndPoint.Name == "KXYZ");
            legEndingAtRefuel.RemainingFuelGals.Should().BeApproximately(full, 0.1);
        }

        [Fact]
        public async Task CalculateNavlog_ShouldPreserveAltitude_ForRefuelStops()
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var refuelAltitude = 700;
            var waypoints = new List<WaypointDto>
            {
                CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                new WaypointDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "KXYZ",
                    Latitude = 35.5,
                    Longitude = -86.0,
                    Altitude = refuelAltitude,
                    WaypointType = WaypointType.Airport,
                    IsRefuelingStop = true,
                    RefuelToFull = true
                },
                CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
            };

            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = waypoints,
                PlannedCruisingAltitude = 7500,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);
            var windsAloftData = GenerateWindsAloftData(departureTime);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            var legEndingAtRefuel = result.Legs.First(l => l.LegEndPoint.Name == "KXYZ");
            legEndingAtRefuel.LegEndPoint.Altitude.Should().Be(refuelAltitude);
        }

        [Fact]
        public async Task CalculateNavlog_TotalFuelUsed_IncludesSTTPerDepartureSegment()
        {
            // Arrange
            var departureTime = DateTime.UtcNow;
            var waypoints = new List<WaypointDto>
            {
                CreateSpecificWaypoint("KBNA", 36.1244, -86.6782, 599),
                new WaypointDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "KXYZ",
                    Latitude = 35.5,
                    Longitude = -86.0,
                    Altitude = 700,
                    WaypointType = WaypointType.Airport,
                    IsRefuelingStop = true,
                    RefuelToFull = true
                },
                CreateSpecificWaypoint("KATL", 33.6367, -84.4278, 1026)
            };

            var request = new NavlogRequestDto
            {
                TimeOfDeparture = departureTime,
                Waypoints = waypoints,
                PlannedCruisingAltitude = 7500,
                AircraftPerformanceProfileId = "test-profile-id"
            };

            _magneticVariationService.GetMagneticVariation(Arg.Any<double>(), Arg.Any<double>()).Returns(0);
            var windsAloftData = GenerateWindsAloftData(departureTime);
            _windsAloftService.FetchWindsAloftData(Arg.Any<int>()).Returns(windsAloftData);

            // Act
            var result = await _navlogService.CalculateNavlog(request);

            // Assert
            var totalLegBurn = result.Legs.Sum(l => l.LegFuelBurnGals);
            var stt = 3.0; // from test profile
            var expectedTotalFuelUsed = totalLegBurn + (stt * 2); // initial + one refuel
            result.TotalFuelUsed.Should().BeApproximately(expectedTotalFuelUsed, 0.25);
        }

        // Helper method to create a waypoint with specific values when needed
        private WaypointDto CreateSpecificWaypoint(string name, double lat, double lon, double altitude,
                WaypointType waypointType = WaypointType.Airport)
        {
            return new WaypointDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Latitude = lat,
                Longitude = lon,
                Altitude = altitude,
                WaypointType = waypointType
            };
        }

        private WindsAloftDto GenerateWindsAloftData(DateTime referenceTime, int? overrideWindDirection = null, int? overrideWindSpeed = null)
        {
            var altitudeLevels = new[] { "3000", "6000", "9000", "12000", "18000", "24000", "30000", "34000", "39000" };

            // Real winds aloft data sample
            var realWindsAloftData = new Dictionary<string, Dictionary<string, WindTempDto>>
            {
                { "KBNA", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 215, Speed = 9, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 215, Speed = 6, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 215, Speed = 7, Temperature = -2 } },
                        { "12000", new WindTempDto { Direction = 216, Speed = 1, Temperature = -16 } },
                        { "18000", new WindTempDto { Direction = 226, Speed = 4, Temperature = -27 } },
                        { "24000", new WindTempDto { Direction = 243, Speed = 9, Temperature = -41 } },
                        { "30000", new WindTempDto { Direction = 245, Speed = 5, Temperature = -51 } },
                        { "34000", new WindTempDto { Direction = 248, Speed = 6, Temperature = -62 } }
                    }
                },
                { "KATL", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 194, Speed = 4, Temperature = 11 } },
                        { "6000", new WindTempDto { Direction = 204, Speed = 4, Temperature = 7 } },
                        { "9000", new WindTempDto { Direction = 215, Speed = 3, Temperature = 2 } },
                        { "12000", new WindTempDto { Direction = 224, Speed = 5, Temperature = -15 } },
                        { "18000", new WindTempDto { Direction = 235, Speed = 5, Temperature = -26 } },
                        { "24000", new WindTempDto { Direction = 256, Speed = 8, Temperature = -40 } },
                        { "30000", new WindTempDto { Direction = 266, Speed = 2, Temperature = -49 } },
                        { "34000", new WindTempDto { Direction = 246, Speed = 6, Temperature = -58 } }
                    }
                },
                { "KJFK", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KBOS", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KSFO", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KLAX", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KDEN", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KPHX", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KMIA", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KSEA", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                },
                { "KPDX", new Dictionary<string, WindTempDto>
                    {
                        { "3000", new WindTempDto { Direction = 210, Speed = 10, Temperature = 10 } },
                        { "6000", new WindTempDto { Direction = 220, Speed = 15, Temperature = 5 } },
                        { "9000", new WindTempDto { Direction = 230, Speed = 20, Temperature = 0 } },
                        { "12000", new WindTempDto { Direction = 240, Speed = 25, Temperature = -5 } },
                        { "18000", new WindTempDto { Direction = 250, Speed = 30, Temperature = -10 } },
                        { "24000", new WindTempDto { Direction = 260, Speed = 35, Temperature = -20 } },
                        { "30000", new WindTempDto { Direction = 270, Speed = 40, Temperature = -30 } },
                        { "34000", new WindTempDto { Direction = 280, Speed = 45, Temperature = -40 } }
                    }
                }
            };

            // Generate a list of wind aloft sites
            var sites = new List<WindsAloftSiteDto>();

            foreach (var airport in realWindsAloftData)
            {
                sites.Add(new WindsAloftSiteDto
                {
                    Id = airport.Key,
                    Lat = airport.Key switch
                    {
                        "KBNA" => 36.12f,
                        "KATL" => 33.64f,
                        "KJFK" => 40.64f,
                        "KBOS" => 42.36f,
                        "KSFO" => 37.62f,
                        "KLAX" => 33.94f,
                        "KDEN" => 39.86f,
                        "KPHX" => 33.43f,
                        "KMIA" => 25.79f,
                        "KSEA" => 47.44f,
                        "KPDX" => 45.60f,
                        _ => 0f
                    },
                    Lon = airport.Key switch
                    {
                        "KBNA" => -86.68f,
                        "KATL" => -84.43f,
                        "KJFK" => -73.78f,
                        "KBOS" => -71.01f,
                        "KSFO" => -122.38f,
                        "KLAX" => -118.41f,
                        "KDEN" => -104.67f,
                        "KPHX" => -112.01f,
                        "KMIA" => -80.29f,
                        "KSEA" => -122.31f,
                        "KPDX" => -122.61f,
                        _ => 0f
                    },
                    WindTemp = airport.Value
                });
            }

            // Finally, create the main WindsAloftDto
            return new WindsAloftDto
            {
                ValidTime = referenceTime,
                ForUseStartTime = referenceTime.AddHours(-3),
                ForUseEndTime = referenceTime.AddHours(3),
                WindTemp = sites
            };
        }
        }
}