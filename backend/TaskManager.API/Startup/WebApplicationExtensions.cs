using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TaskManager.API.Grpc;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Services;

namespace TaskManager.API.Startup;

/// <summary>
/// Every app.Use*/app.Map* (and pre-run startup) step Program.cs needs, grouped by concern.
/// Mirrors <see cref="WebApplicationBuilderExtensions"/> for the pre-Build() half.
/// </summary>
public static class WebApplicationExtensions
{
    // Production safety net (PREP.4) — turn a forgotten env var into a clear startup error
    // instead of a silent fallback to dev-only defaults. Development/other environments
    // are unaffected.
    public static void ConfigureProductionSafetyChecks(this WebApplication app, JwtSettings jwtSettings)
    {
        if (!app.Environment.IsProduction())
            return;

        var connectionString = WebApplicationBuilderExtensions.ResolveConnectionString(app.Configuration, app.Services);
        if (connectionString == WebApplicationBuilderExtensions.DevFallbackConnectionString)
        {
            throw new InvalidOperationException(
                "No database connection string configured for Production. " +
                "Set the DatabaseSettings__ConnectionString environment variable.");
        }

        if (string.IsNullOrEmpty(jwtSettings.Secret))
        {
            throw new InvalidOperationException(
                "No JWT signing secret configured for Production. " +
                "Set the Jwt__Secret environment variable.");
        }
    }

    // Converts ValidationException → 400, InvalidOperationException → 422,
    // ForbiddenAccessException → 403, anything else → 500.
    public static void ConfigureExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                if (feature?.Error is ValidationException validationEx)
                {
                    context.Response.StatusCode  = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    var errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    await context.Response.WriteAsJsonAsync(new { errors });
                }
                else if (feature?.Error is InvalidOperationException opEx)
                {
                    context.Response.StatusCode  = StatusCodes.Status422UnprocessableEntity;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { error = opEx.Message });
                }
                else if (feature?.Error is ForbiddenAccessException forbiddenEx)
                {
                    context.Response.StatusCode  = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { error = forbiddenEx.Message });
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            });
        });
    }

    public static async Task MigrateAndSeedDatabaseAsync(this WebApplication app, IAllowedOriginCache allowedOriginCache)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db, logger);

        if (app.Environment.IsDevelopment())
            await DataSeeder.SeedDevelopmentDataAsync(db, logger);

        var seededOrigins = await db.AllowedOrigins.Select(o => o.OriginUrl).ToListAsync();
        allowedOriginCache.SetOrigins(seededOrigins);
    }

    public static void ConfigureDevelopmentTools(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title             = "TaskManager API Reference";
            options.Theme             = ScalarTheme.DeepSpace;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });
    }

    public static void ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseCors("Default");

        // Serves uploaded avatars (wwwroot/avatars/{id}.{ext}) back out at /avatars/{id}.{ext},
        // matching AppUser.AvatarUrl's local-hosting convention. Anonymous by design — avatar
        // images aren't sensitive, and gating them behind auth would break plain <img> tags.
        app.UseStaticFiles();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void ConfigureEndpoints(this WebApplication app)
    {
        // No [Authorize] — Docker's healthcheck can't carry a JWT.
        app.MapHealthChecks("/health").AllowAnonymous();

        app.MapControllers();

        // gRPC services (MCP01.3/MCP01.4) — bound only to the second Kestrel endpoint in
        // practice (nothing routes REST traffic to a gRPC content-type), each carrying
        // [Authorize] to match the REST controllers' auth posture.
        app.MapGrpcService<ProjectsGrpcService>();
        app.MapGrpcService<EpicsGrpcService>();
        app.MapGrpcService<TasksGrpcService>();
        app.MapGrpcService<PhasesGrpcService>();
    }
}
