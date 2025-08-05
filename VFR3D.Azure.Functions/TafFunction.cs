using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions;

public class TafFunction
{
    private readonly ILogger _logger;
    private readonly IAviationWeatherService<Taf> _tafService;

    public TafFunction(IAviationWeatherService<Taf> tafService, ILoggerFactory loggerFactory)
    {
        _tafService = tafService ?? throw new ArgumentNullException(nameof(tafService));
        _logger = loggerFactory.CreateLogger<TafFunction>();
    }

    [Function("TafFunction")]
    public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("TAF Function executed at: {time}", DateTime.UtcNow);
        var cancellationToken = context.CancellationToken;

        try
        {
            await _tafService.PollWeatherDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing TAF service");
            throw;
        }
    }
}