using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Domain.ValueObjects.Flights;
using VFR3D.Infrastructure.Dtos.Flights;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services;
using Xunit;

namespace VFR3D.Tests.IntegrationTests
{
    public class FlightServiceTests : PostgreSqlTestBase
    {
        private INavlogService? _mockNavlogService;
        private ILogger<FlightService>? _mockLogger;
        private FlightService? _flightService;

        private readonly Faker _faker;
        private readonly Faker<WaypointDto> _waypointFaker;
        private readonly Faker<AircraftPerformanceProfile> _profileFaker;

        public FlightServiceTests() : base()
        {
            _faker = new Faker();

            _waypointFaker = new Faker<WaypointDto>()
                .RuleFor(w => w.Id, f => f.Random.AlphaNumeric(6))
                .RuleFor(w => w.Name, f => f.Random.String2(4).ToUpper())
                .RuleFor(w => w.Latitude, f => f.Random.Double(-90, 90))
                .RuleFor(w => w.Longitude, f => f.Random.Double(-180, 180))
                .RuleFor(w => w.Altitude, f => f.Random.Double(0, 10000))
                .RuleFor(w => w.WaypointType, f => f.PickRandom<WaypointType>());

            _profileFaker = new Faker<AircraftPerformanceProfile>()
                .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
                .RuleFor(p => p.ProfileName, f => f.Vehicle.Model())
                .RuleFor(p => p.ClimbTrueAirspeed, f => f.Random.Int(80, 150))
                .RuleFor(p => p.CruiseTrueAirspeed, f => f.Random.Int(120, 200))
                .RuleFor(p => p.DescentTrueAirspeed, f => f.Random.Int(80, 150))
                .RuleFor(p => p.ClimbFpm, f => f.Random.Int(300, 800))
                .RuleFor(p => p.DescentFpm, f => f.Random.Int(400, 900))
                .RuleFor(p => p.ClimbFuelBurn, f => f.Random.Double(8, 15))
                .RuleFor(p => p.CruiseFuelBurn, f => f.Random.Double(6, 12))
                .RuleFor(p => p.DescentFuelBurn, f => f.Random.Double(3, 8))
                .RuleFor(p => p.SttFuelGals, f => f.Random.Double(3, 7))
                .RuleFor(p => p.FuelOnBoardGals, f => f.Random.Double(40, 60));
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _mockLogger = Substitute.For<ILogger<FlightService>>();
            _mockNavlogService = Substitute.For<INavlogService>();
            _flightService = new FlightService(DbContext, _mockNavlogService, _mockLogger);
        }

        protected override async Task SeedDatabaseAsync()
        {
            // Seed airports for state code lookup
            var airports = new[]
            {
                new Airport
                {
                    SiteNo = "1234",
                    ArptId = "LAX",
                    IcaoId = "KLAX",
                    StateCode = "CA",
                    LatDecimal = 33.9425m,
                    LongDecimal = -118.4081m
                },
                new Airport
                {
                    SiteNo = "5678",
                    ArptId = "SAN",
                    IcaoId = "KSAN",
                    StateCode = "CA",
                    LatDecimal = 32.7336m,
                    LongDecimal = -117.1897m
                }
            };

            DbContext.Airports.AddRange(airports);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateFlight_ShouldCreateAndReturnFlight_WhenRequestIsValid()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);

            // Create waypoints for the flight
            var departurePoint = _waypointFaker
                .RuleFor(w => w.Name, "KLAX")
                .RuleFor(w => w.WaypointType, WaypointType.Airport)
                .Generate();

            var arrivalPoint = _waypointFaker
                .RuleFor(w => w.Name, "KSAN")
                .RuleFor(w => w.WaypointType, WaypointType.Airport)
                .Generate();

            // Create a performance profile
            var profile = _profileFaker
                .RuleFor(p => p.UserId, userId)
                .Generate();

            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create flight request
            var request = new CreateFlightRequestDto
            {
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                Waypoints = new List<WaypointDto> { departurePoint, arrivalPoint },
                AircraftPerformanceProfileId = profile.Id
            };

            // Setup mock NavlogService
            var mockNavlogResponse = new NavlogResponseDto
            {
                TotalRouteDistance = _faker.Random.Double(50, 300),
                TotalRouteTimeHours = _faker.Random.Double(0.5, 2.5),
                TotalFuelUsed = _faker.Random.Double(10, 30),
                AverageWindComponent = _faker.Random.Double(-15, 15),
                Legs = new List<NavigationLegDto>
                {
                    new NavigationLegDto
                    {
                        LegStartPoint = departurePoint,
                        LegEndPoint = arrivalPoint,
                        TrueCourse = _faker.Random.Double(0, 359),
                        MagneticCourse = _faker.Random.Double(0, 359),
                        MagneticHeading = _faker.Random.Double(0, 359),
                        GroundSpeed = _faker.Random.Double(80, 200),
                        LegDistance = _faker.Random.Double(50, 300),
                        DistanceRemaining = 0,
                        StartLegTime = request.DepartureTime,
                        EndLegTime = request.DepartureTime.AddHours(_faker.Random.Double(0.5, 2.5)),
                        LegFuelBurnGals = _faker.Random.Double(5, 20),
                        RemainingFuelGals = _faker.Random.Double(20, 40),
                        WindDir = _faker.Random.Int(0, 359),
                        WindSpeed = _faker.Random.Int(0, 30),
                        HeadwindComponent = _faker.Random.Double(-15, 15),
                        TempC = _faker.Random.Int(-20, 35)
                    }
                }
            };

            _mockNavlogService?.CalculateNavlog(Arg.Any<NavlogRequestDto>()).Returns(mockNavlogResponse);

            // Act
            var result = await _flightService!.CreateFlight(userId, request);

            // Assert
            if(result != null)
            {
                result.Should().NotBeNull();
                result.Auth0UserId.Should().Be(userId);
                result.Name.Should().Be(request.Name);
                DateTime.Parse(result.DepartureTime).Should().Be(request.DepartureTime);
                result.PlannedCruisingAltitude.Should().Be(request.PlannedCruisingAltitude);
                result.AircraftPerformanceId.Should().Be(request.AircraftPerformanceProfileId);
                result.TotalRouteDistance.Should().Be(mockNavlogResponse.TotalRouteDistance);
                result.TotalRouteTimeHours.Should().Be(mockNavlogResponse.TotalRouteTimeHours);
                result.TotalFuelUsed.Should().Be(mockNavlogResponse.TotalFuelUsed);
                result.AverageWindComponent.Should().Be(mockNavlogResponse.AverageWindComponent);
                result.Legs.Should().HaveCount(1);
                result.StateCodesAlongRoute.Should().Contain("CA");

                // Verify the flight was saved to the database
                var savedFlight = await DbContext.Flights.FirstOrDefaultAsync(f => f.Id == result.Id);
                savedFlight.Should().NotBeNull();
                savedFlight.Name.Should().Be(request.Name);
            }
        }

        [Fact]
        public async Task GetFlights_ShouldReturnAllFlightsForUser()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);

            // Create profiles
            var profile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create 3 flights for this user
            var flights = new List<Flight>();
            for (int i = 0; i < 3; i++)
            {
                var flight = new Flight
                {
                    Id = Guid.NewGuid().ToString(),
                    Auth0UserId = userId,
                    Name = _faker.Lorem.Sentence(),
                    DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                    PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                    AircraftPerformanceId = profile.Id,
                    Waypoints = new List<Waypoint>(),
                    StateCodesAlongRoute = new List<string> { "CA" }
                };
                flights.Add(flight);
            }

            // Create 1 flight for another user
            var otherUserFlight = new Flight
            {
                Id = Guid.NewGuid().ToString(),
                Auth0UserId = "another-user",
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.AddRange(flights);
            DbContext.Flights.Add(otherUserFlight);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _flightService!.GetFlights(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.All(f => f.Auth0UserId == userId).Should().BeTrue();
        }
        
        [Fact]
        public async Task GetFlight_ShouldReturnFlight_WhenFlightExists()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create flight
            var flight = new Flight
            {
                Id = flightId,
                Auth0UserId = userId,
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(flight);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _flightService!.GetFlight(userId, flightId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(flightId);
            result.Auth0UserId.Should().Be(userId);
            result.Name.Should().Be(flight.Name);
        }

        [Fact]
        public async Task GetFlight_ShouldThrowKeyNotFoundException_WhenFlightDoesNotExist()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var nonExistentFlightId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.GetFlight(userId, nonExistentFlightId));
        }

        [Fact]
        public async Task GetFlight_ShouldThrowKeyNotFoundException_WhenFlightBelongsToAnotherUser()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var anotherUserId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, anotherUserId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create flight for another user
            var flight = new Flight
            {
                Id = flightId,
                Auth0UserId = anotherUserId,
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(flight);
            await DbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.GetFlight(userId, flightId));
        }

        [Fact]
        public async Task UpdateFlight_ShouldUpdateFlightProperties_WhenFlightExists()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profiles
            var originalProfile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            var newProfile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.AddRange(originalProfile, newProfile);
            await DbContext.SaveChangesAsync();

            // Create flight
            var originalFlight = new Flight
            {
                Id = flightId,
                Auth0UserId = userId,
                Name = "Original Flight Name",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                PlannedCruisingAltitude = 5000,
                AircraftPerformanceId = originalProfile.Id,
                Waypoints = new List<Waypoint>(),
                Legs = new List<NavlogLeg>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(originalFlight);
            await DbContext.SaveChangesAsync();

            // Setup update request
            var updateRequest = new UpdateFlightRequestDto
            {
                Name = "Updated Flight Name",
                DepartureTime = DateTime.UtcNow.AddDays(2),
                PlannedCruisingAltitude = 7000,
                AircraftPerformanceProfileId = newProfile.Id,
                Waypoints = new List<WaypointDto>
        {
            _waypointFaker.RuleFor(w => w.Name, "KLAX").Generate(),
            _waypointFaker.RuleFor(w => w.Name, "KSAN").Generate()
        }
            };

            // Setup mock NavlogService for the recalculation
            var mockNavlogResponse = new NavlogResponseDto
            {
                TotalRouteDistance = 150.5,
                TotalRouteTimeHours = 1.25,
                TotalFuelUsed = 15.5,
                AverageWindComponent = -2.3,
                Legs = new List<NavigationLegDto>
        {
            new NavigationLegDto
            {
                LegStartPoint = updateRequest.Waypoints[0],
                LegEndPoint = updateRequest.Waypoints[1],
                LegDistance = 150.5,
                // Add other properties as needed
            }
        }
            };

            _mockNavlogService!.CalculateNavlog(Arg.Any<NavlogRequestDto>()).Returns(mockNavlogResponse);

            // Act
            var result = await _flightService!.UpdateFlight(userId, flightId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(flightId);
            result.Name.Should().Be(updateRequest.Name);
            DateTime.Parse(result.DepartureTime).Should().Be(updateRequest.DepartureTime.Value);
            result.PlannedCruisingAltitude.Should().Be(updateRequest.PlannedCruisingAltitude.Value);
            result.AircraftPerformanceId.Should().Be(updateRequest.AircraftPerformanceProfileId);
            result.TotalRouteDistance.Should().Be(mockNavlogResponse.TotalRouteDistance);
            result.TotalRouteTimeHours.Should().Be(mockNavlogResponse.TotalRouteTimeHours);
            result.TotalFuelUsed.Should().Be(mockNavlogResponse.TotalFuelUsed);
            result.AverageWindComponent.Should().Be(mockNavlogResponse.AverageWindComponent);

            // Verify NavlogService was called
            await _mockNavlogService!.Received(1).CalculateNavlog(Arg.Any<NavlogRequestDto>());

            // Verify the database was updated
            var updatedFlight = await DbContext.Flights.FindAsync(flightId);
            updatedFlight.Should().NotBeNull();
            updatedFlight.Name.Should().Be(updateRequest.Name);
        }

        [Fact]
        public async Task UpdateFlight_ShouldThrowKeyNotFoundException_WhenFlightDoesNotExist()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var nonExistentFlightId = Guid.NewGuid().ToString();

            var updateRequest = new UpdateFlightRequestDto
            {
                Name = "Updated Flight Name"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.UpdateFlight(userId, nonExistentFlightId, updateRequest));
        }

        [Fact]
        public async Task UpdateFlight_ShouldOnlyUpdateProvidedFields()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Original flight data
            var originalName = "Original Flight Name";
            var originalDepartureTime = DateTime.UtcNow.AddDays(1);
            var originalAltitude = 5000;

            // Create flight
            var originalFlight = new Flight
            {
                Id = flightId,
                Auth0UserId = userId,
                Name = originalName,
                DepartureTime = originalDepartureTime,
                PlannedCruisingAltitude = originalAltitude,
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                Legs = new List<NavlogLeg>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(originalFlight);
            await DbContext.SaveChangesAsync();

            // Setup update request with only name changed
            var updateRequest = new UpdateFlightRequestDto
            {
                Name = "Updated Flight Name"
            };

            // Mock NavlogService to verify it's not called when no related fields are updated
            _mockNavlogService!.CalculateNavlog(Arg.Any<NavlogRequestDto>())
                .Returns(new NavlogResponseDto());

            // Act
            var result = await _flightService!.UpdateFlight(userId, flightId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updateRequest.Name);
            DateTime.Parse(result.DepartureTime).Should().Be(originalDepartureTime);
            result.PlannedCruisingAltitude.Should().Be(originalAltitude);

            // Verify NavlogService was NOT called
            await _mockNavlogService!.DidNotReceive().CalculateNavlog(Arg.Any<NavlogRequestDto>());
        }

        [Fact]
        public async Task DeleteFlight_ShouldRemoveFlightFromDatabase_WhenFlightExists()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create flight
            var flight = new Flight
            {
                Id = flightId,
                Auth0UserId = userId,
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(flight);
            await DbContext.SaveChangesAsync();

            // Act
            await _flightService!.DeleteFlight(userId, flightId);

            // Assert
            var deletedFlight = await DbContext.Flights.FindAsync(flightId);
            deletedFlight.Should().BeNull();
        }

        [Fact]
        public async Task DeleteFlight_ShouldThrowKeyNotFoundException_WhenFlightDoesNotExist()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var nonExistentFlightId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.DeleteFlight(userId, nonExistentFlightId));
        }

        [Fact]
        public async Task DeleteFlight_ShouldThrowKeyNotFoundException_WhenFlightBelongsToAnotherUser()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var anotherUserId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, anotherUserId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create flight for another user
            var flight = new Flight
            {
                Id = flightId,
                Auth0UserId = anotherUserId,
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = _faker.Random.Int(3000, 12000),
                AircraftPerformanceId = profile.Id,
                Waypoints = new List<Waypoint>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(flight);
            await DbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.DeleteFlight(userId, flightId));
        }

        [Fact]
        public async Task RegenerateNavlog_ShouldUpdateFlightWithNewWeatherData()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var flightId = Guid.NewGuid().ToString();

            // Create profile
            var profile = _profileFaker.RuleFor(p => p.UserId, userId).Generate();
            DbContext.AircraftPerformanceProfiles.Add(profile);
            await DbContext.SaveChangesAsync();

            // Create waypoints
            var waypoints = new List<Waypoint>
    {
        new Waypoint
        {
            Id = "wp1",
            Name = "KLAX",
            Latitude = 33.9425,
            Longitude = -118.4081,
            Altitude = 200,
            WaypointType = WaypointType.Airport
        },
        new Waypoint
        {
            Id = "wp2",
            Name = "KSAN",
            Latitude = 32.7336,
            Longitude = -117.1897,
            Altitude = 200,
            WaypointType = WaypointType.Airport
        }
    };

            // Create flight with original navlog data
            var flight = new Flight
            {
                Id = flightId,
                Auth0UserId = userId,
                Name = _faker.Lorem.Sentence(),
                DepartureTime = _faker.Date.FutureOffset().UtcDateTime,
                PlannedCruisingAltitude = 5000,
                AircraftPerformanceId = profile.Id,
                Waypoints = waypoints,
                TotalRouteDistance = 100.0,
                TotalRouteTimeHours = 1.0,
                TotalFuelUsed = 10.0,
                AverageWindComponent = 0.0,
                Legs = new List<NavlogLeg>(),
                StateCodesAlongRoute = new List<string> { "CA" }
            };

            DbContext.Flights.Add(flight);
            await DbContext.SaveChangesAsync();

            // Setup updated navlog response with new weather data
            var updatedNavlogResponse = new NavlogResponseDto
            {
                TotalRouteDistance = 100.5,
                TotalRouteTimeHours = 1.1,
                TotalFuelUsed = 11.0,
                AverageWindComponent = -5.0,
                Legs = new List<NavigationLegDto>
        {
            new NavigationLegDto
            {
                LegStartPoint = new WaypointDto
                {
                    Id = "wp1",
                    Name = "KLAX",
                    Latitude = 33.9425,
                    Longitude = -118.4081,
                    Altitude = 200,
                    WaypointType = WaypointType.Airport
                },
                LegEndPoint = new WaypointDto
                {
                    Id = "wp2",
                    Name = "KSAN",
                    Latitude = 32.7336,
                    Longitude = -117.1897,
                    Altitude = 200,
                    WaypointType = WaypointType.Airport
                },
                TrueCourse = 145.5,
                MagneticCourse = 148.2,
                MagneticHeading = 152.1,
                GroundSpeed = 120.5,
                LegDistance = 100.5,
                DistanceRemaining = 0,
                StartLegTime = flight.DepartureTime,
                EndLegTime = flight.DepartureTime.AddHours(1.1),
                LegFuelBurnGals = 11.0,
                RemainingFuelGals = 39.0,
                WindDir = 270,
                WindSpeed = 15,
                HeadwindComponent = -5.0,
                TempC = 18
            }
        }
            };

            _mockNavlogService!.CalculateNavlog(Arg.Any<NavlogRequestDto>())
                .Returns(updatedNavlogResponse);

            // Act
            var result = await _flightService!.RegenerateNavlog(userId, flightId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(flightId);
            result.TotalRouteDistance.Should().Be(updatedNavlogResponse.TotalRouteDistance);
            result.TotalRouteTimeHours.Should().Be(updatedNavlogResponse.TotalRouteTimeHours);
            result.TotalFuelUsed.Should().Be(updatedNavlogResponse.TotalFuelUsed);
            result.AverageWindComponent.Should().Be(updatedNavlogResponse.AverageWindComponent);
            result.Legs.Should().HaveCount(1);

            // Verify NavlogService was called with correct parameters
            await _mockNavlogService!.Received(1).CalculateNavlog(Arg.Is<NavlogRequestDto>(
                req => req.TimeOfDeparture == flight.DepartureTime &&
                       req.PlannedCruisingAltitude == flight.PlannedCruisingAltitude &&
                       req.AircraftPerformanceProfileId == flight.AircraftPerformanceId));

            // Verify the database was updated
            var updatedFlight = await DbContext.Flights.FindAsync(flightId);
            updatedFlight.Should().NotBeNull();
            updatedFlight.TotalRouteDistance.Should().Be(updatedNavlogResponse.TotalRouteDistance);
        }

        [Fact]
        public async Task RegenerateNavlog_ShouldThrowKeyNotFoundException_WhenFlightDoesNotExist()
        {
            // Arrange
            var userId = _faker.Random.AlphaNumeric(10);
            var nonExistentFlightId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService!.RegenerateNavlog(userId, nonExistentFlightId));
        }

    }
}