using VFR3D.Infrastructure.Dtos.WeightBalance;

namespace VFR3D.Infrastructure.Interfaces;

public interface IWeightBalanceProfileService
{
    Task<WeightBalanceProfileDto> CreateProfile(string userId, CreateWeightBalanceProfileRequestDto request);
    Task<WeightBalanceProfileDto?> GetProfile(string userId, Guid profileId);
    Task<List<WeightBalanceProfileDto>> GetProfilesByUser(string userId);
    Task<List<WeightBalanceProfileDto>> GetProfilesByAircraft(string userId, string aircraftId);
    Task<WeightBalanceProfileDto> UpdateProfile(string userId, Guid profileId, UpdateWeightBalanceProfileRequestDto request);
    Task DeleteProfile(string userId, Guid profileId);
    Task<WeightBalanceCalculationResultDto> Calculate(string userId, Guid profileId, WeightBalanceCalculationRequestDto request);
}
