using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Infrastructure.Services;

public class AutomationWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutomationWorker> _logger;

    public AutomationWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AutomationWorker> logger)
    {
        _scopeFactory  = scopeFactory;
        _configuration = configuration;
        _logger        = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutomationWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await AutoArchiveCompletedTasks(stoppingToken);

            // This is where we tap into local MQTT publishing or local AI engines later.

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("AutomationWorker background task is stopping.");
    }

    // ponytail: one instance-wide setting (TaskArchiving__AutoArchiveAfterDays, unset = off);
    // make it per-project once the U06 project settings page exists.
    private async Task AutoArchiveCompletedTasks(CancellationToken ct)
    {
        var days = _configuration.GetValue<int?>("TaskArchiving:AutoArchiveAfterDays");
        if (days is not > 0) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var count = await mediator.Send(new ArchiveCompletedTasksCommand(null, days.Value), ct);
            if (count > 0)
                _logger.LogInformation("Auto-archived {Count} tasks completed more than {Days} days ago.", count, days);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A failed pass must not kill the worker; the next pass retries.
            _logger.LogError(ex, "Automatic task archiving failed.");
        }
    }
}
