using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions;

public class PirepFunction
{
    private readonly ILogger _logger;
    private readonly IAviationWeatherService<Pirep> _pirepService;

    public PirepFunction(IAviationWeatherService<Pirep> pirepService, ILoggerFactory loggerFactory)
    {
        _pirepService = pirepService ?? throw new ArgumentNullException(nameof(pirepService));
        _logger = loggerFactory.CreateLogger<PirepFunction>();
    }

    [Function("PirepFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation($"PirepFunction executed at: {DateTime.Now}");
        try
        {
            await _pirepService.PollWeatherDataAsync(context.CancellationToken);
            _logger.LogInformation("Pirep data processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing Pirep data.");
        }
    }
}