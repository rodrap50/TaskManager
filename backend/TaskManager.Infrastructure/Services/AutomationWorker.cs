using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskManager.Infrastructure.Services;

public class AutomationWorker : BackgroundService
{
    private readonly ILogger<AutomationWorker> _logger;

    public AutomationWorker(ILogger<AutomationWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutomationWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AutomationWorker task doing background work at: {time}", DateTimeOffset.Now);
            
            // This is where we tap into local MQTT publishing or local AI engines later.
            
            await Task.Delay(10000, stoppingToken);
        }
        
        _logger.LogInformation("AutomationWorker background task is stopping.");
    }
}
