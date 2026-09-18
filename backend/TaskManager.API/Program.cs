using Serilog;
using TaskManager.API.Startup;

namespace TaskManager.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting TaskManager API");

        try
        {
            await Run(args);
            Log.Information("TaskManager API stopped cleanly");
        }
        catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "StopTheHostException")
        {
            Log.Fatal(ex, "TaskManager API terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureLogging();
        builder.ConfigureBuiltInServices();
        builder.ConfigureDatabase();
        builder.ConfigureCustomServices();
        var allowedOriginCache = builder.ConfigureCors();
        var jwtSettings = builder.ConfigureAuthentication();
        builder.ConfigureGrpc();

        var app = builder.Build();

        app.ConfigureProductionSafetyChecks(jwtSettings);
        app.ConfigureExceptionHandling();
        await app.MigrateAndSeedDatabaseAsync(allowedOriginCache);
        app.ConfigureDevelopmentTools();
        app.ConfigureMiddlewarePipeline();
        app.ConfigureEndpoints();

        await app.RunAsync();
    }
}
