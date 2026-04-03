using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services;

public class PreflightApiClient : IPreflightApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PreflightApiClient> _logger;

    public PreflightApiClient(
        HttpClient httpClient,
        IOptions<PreflightApiSettings> settings,
        ILogger<PreflightApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);

        if (!string.IsNullOrEmpty(settings.Value.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", settings.Value.ApiKey);
        }
    }

    public async Task<NavlogResponseDto> CalculateNavlogAsync(
        NavlogRequestDto request,
        NavlogPerformanceDataDto performanceData)
    {
        var preflightRequest = new PreflightNavlogRequestDto
        {
            Waypoints = request.Waypoints,
            PlannedCruisingAltitude = request.PlannedCruisingAltitude,
            TimeOfDeparture = request.TimeOfDeparture,
            PerformanceData = performanceData
        };

        _logger.LogInformation("Calling preflight.api navlog calculation with {WaypointCount} waypoints",
            request.Waypoints.Count);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/navlog/calculate", preflightRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Preflight API navlog calculation failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);
            throw new ExternalServiceException(
                "PreflightApi",
                $"Navlog calculation failed with status {response.StatusCode}: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<NavlogResponseDto>();

        if (result == null)
        {
            throw new ExternalServiceException("PreflightApi", "Navlog calculation returned null response");
        }

        return result;
    }
}
