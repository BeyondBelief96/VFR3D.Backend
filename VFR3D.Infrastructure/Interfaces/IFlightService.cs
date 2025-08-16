using VFR3D.Infrastructure.Dtos.Flights;

namespace VFR3D.Infrastructure.Interfaces;

public interface IFlightService
{
    Task<FlightDto> CreateFlight(string userId, CreateFlightRequestDto request);
    Task<FlightDto> UpdateFlight(string userId, string flightId, UpdateFlightRequestDto request);
    Task<List<FlightDto>> GetFlights(string userId);
    Task<FlightDto> GetFlight(string userId, string flightId);
    Task DeleteFlight(string userId, string flightId);
    Task<FlightDto> RegenerateNavlog(string userId, string flightId);
    Task<(FlightDto Outbound, FlightDto Return)> CreateRoundTripFlight(string userId, CreateRoundTripFlightRequestDto request);
}