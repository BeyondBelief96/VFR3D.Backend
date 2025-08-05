using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Vfr3d.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions;

public class MetarFunction
{
    private readonly ILogger _logger;
    private readonly IAviationWeatherService<Metar> _metarService;

    public MetarFunction(IAviationWeatherService<Metar> metarService, ILoggerFactory loggerFactory)
    {
        _metarService = metarService ?? throw new ArgumentNullException(nameof(metarService));
        _logger = loggerFactory.CreateLogger<MetarFunction>();
    }

    [Function("MetarFunction")]
    public async Task Run([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("METAR Function executed at: {time}", DateTime.UtcNow);
        var cancellationToken = context.CancellationToken;

        try
        {
            await _metarService.PollWeatherDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing METAR service");
            throw;
        }
    }
}