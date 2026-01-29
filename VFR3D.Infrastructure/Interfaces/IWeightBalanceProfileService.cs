using VFR3D.Infrastructure.Dtos.WeightBalance;

namespace VFR3D.Infrastructure.Interfaces;

public interface IWeightBalanceProfileService
{
    // Profile CRUD operations
    Task<WeightBalanceProfileDto> CreateProfile(string userId, CreateWeightBalanceProfileRequestDto request);
    Task<WeightBalanceProfileDto?> GetProfile(string userId, Guid profileId);
    Task<List<WeightBalanceProfileDto>> GetProfilesByUser(string userId);
    Task<List<WeightBalanceProfileDto>> GetProfilesByAircraft(string userId, string aircraftId);
    Task<WeightBalanceProfileDto> UpdateProfile(string userId, Guid profileId, UpdateWeightBalanceProfileRequestDto request);
    Task DeleteProfile(string userId, Guid profileId);

    // Non-persisted calculation
    Task<WeightBalanceCalculationResultDto> Calculate(string userId, Guid profileId, WeightBalanceCalculationRequestDto request);

    // Persisted calculations
    Task<WeightBalanceCalculationDto> CalculateAndSave(string userId, SaveWeightBalanceCalculationRequestDto request);
    Task<WeightBalanceCalculationDto?> GetCalculation(string userId, Guid calculationId);
    Task<WeightBalanceCalculationDto?> GetCalculationForFlight(string userId, string flightId);
    Task<StandaloneCalculationStateDto?> GetLatestStandaloneState(string userId);
    Task DeleteCalculation(string userId, Guid calculationId);
}
