using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using TaskManager.API.Authentication;
using TaskManager.API.Services;
using TaskManager.Application;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;

namespace TaskManager.API;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

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

        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services)
                         .Enrich.FromLogContext()
                         .Enrich.WithMachineName()
                         .Enrich.WithEnvironmentName()
                         .Enrich.WithThreadId();

            // Friendly env var overrides for the Seq/File sinks (PREP.5) — e.g. SEQ_URL instead
            // of the nested Serilog__WriteTo__N__Args__X syntax, which is fragile since it depends
            // on WriteTo's array order. Both sinks stay always-registered (same as before), using
            // the Seq:Url / FileLogging:Path appsettings defaults unless overridden.
            var seqUrl = context.Configuration["SEQ_URL"] ?? context.Configuration["Seq:Url"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
                configuration.WriteTo.Seq(seqUrl);

            var logFilePath = context.Configuration["LOG_FILE_PATH"] ?? context.Configuration["FileLogging:Path"];
            if (!string.IsNullOrWhiteSpace(logFilePath))
            {
                configuration.WriteTo.File(logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    fileSizeLimitBytes: 104857600,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }
        });

        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title       = "TaskManager API";
                document.Info.Version     = "v1";
                document.Info.Description =
                    "Self-hosted, privacy-first project management API. " +
                    "Exposes endpoints for Projects, Tasks, Users, and the Webhook Integration Gateway.";
                document.Info.Contact = new() { Name = "TaskManager" };
                return Task.CompletedTask;
            });
        });

        // Infrastructure — Database
        const string DevFallbackConnectionString = "Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=postgres";

        string ResolveConnectionString(IServiceProvider services)
        {
            var dbSettings = services.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            return !string.IsNullOrEmpty(dbSettings.ConnectionString)
                ? dbSettings.ConnectionString
                : builder.Configuration.GetConnectionString("DefaultConnection")
                  ?? DevFallbackConnectionString;
        }

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(ResolveConnectionString(serviceProvider));
        });

        // Infrastructure — Repositories + Services
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddSingleton<IAvatarStorage, LocalAvatarStorage>();

        // API — Current-user access for Application-layer handlers
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Application — MediatR + FluentValidation pipeline
        builder.Services.AddApplicationServices();

        // Health check for Docker (D03) — checks the API and its DB connection.
        builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        // Infrastructure — Background Services
        builder.Services.AddHostedService<AutomationWorker>();

        // CORS — DB-backed, admin-editable allow-list (PREP.1). The same singleton instance
        // is registered for DI (so AllowedOrigins command handlers can refresh it after a
        // change) and closed over directly by the policy below, so an admin adding/removing
        // an origin takes effect immediately with no API restart.
        var allowedOriginCache = new AllowedOriginCache();
        builder.Services.AddSingleton<IAllowedOriginCache>(allowedOriginCache);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Default",
                policy => policy.SetIsOriginAllowed(allowedOriginCache.IsAllowed)
                                .AllowAnyMethod()
                                .AllowAnyHeader());
        });

        // Authentication — JWT Bearer, signed with the same key AUTH01.1's login endpoint mints tokens with.
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        // Default scheme is a "smart" policy scheme that forwards to whichever concrete
        // scheme fits the request — a human's JWT or a service account's ApiToken header —
        // so every [Authorize]/FallbackPolicy consumer accepts either without listing schemes.
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme          = "SmartAuth";
            options.DefaultChallengeScheme = "SmartAuth";
        })
            .AddPolicyScheme("SmartAuth", "JWT Bearer or API Token", policyOptions =>
            {
                policyOptions.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(ApiTokenAuthenticationDefaults.HeaderName)
                        ? ApiTokenAuthenticationDefaults.AuthenticationScheme
                        : JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero,
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(
                ApiTokenAuthenticationDefaults.AuthenticationScheme, _ => { });

        // Every endpoint requires an authenticated principal by default; [AllowAnonymous] opts individual
        // endpoints out (currently only the login endpoint itself).
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        var app = builder.Build();

        // Production safety net (PREP.4) — turn a forgotten env var into a clear startup
        // error instead of a silent fallback to dev-only defaults. Development/other
        // environments are unaffected.
        if (app.Environment.IsProduction())
        {
            if (ResolveConnectionString(app.Services) == DevFallbackConnectionString)
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

        // Global exception handling — converts ValidationException → 400, anything else → 500.
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
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

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            await db.Database.MigrateAsync();
            await DataSeeder.SeedAsync(db, logger);

            if (app.Environment.IsDevelopment())
                await DataSeeder.SeedDevelopmentDataAsync(db, logger);

            var seededOrigins = await db.AllowedOrigins.Select(o => o.OriginUrl).ToListAsync();
            allowedOriginCache.SetOrigins(seededOrigins);
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title             = "TaskManager API Reference";
                options.Theme             = ScalarTheme.DeepSpace;
                options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
                options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
            });
        }

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

        // No [Authorize] — Docker's healthcheck can't carry a JWT.
        app.MapHealthChecks("/health").AllowAnonymous();

        app.MapControllers();

        await app.RunAsync();
    }
}
