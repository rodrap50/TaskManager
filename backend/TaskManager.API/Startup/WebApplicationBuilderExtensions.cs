using System.Text;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Threading.RateLimiting;
using TaskManager.API.Authentication;
using TaskManager.API.Middleware;
using TaskManager.API.Services;
using TaskManager.Application;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;

namespace TaskManager.API.Startup;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Every builder.Services/builder.Host/builder.WebHost registration Program.cs needs, grouped
/// by concern. Mirrored by <see cref="WebApplicationExtensions"/> for the post-Build() half.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public const string DevFallbackConnectionString = "Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=postgres";

    // Shared with ConfigureProductionSafetyChecks, which needs the same resolution logic
    // against the already-built app.Services/app.Configuration.
    public static string ResolveConnectionString(IConfiguration configuration, IServiceProvider services)
    {
        var dbSettings = services.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        return !string.IsNullOrEmpty(dbSettings.ConnectionString)
            ? dbSettings.ConnectionString
            : configuration.GetConnectionString("DefaultConnection")
              ?? DevFallbackConnectionString;
    }

    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
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
    }

    public static void ConfigureBuiltInServices(this WebApplicationBuilder builder)
    {
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

        // Health check for Docker (D03) — checks the API and its DB connection.
        builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
    }

    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(ResolveConnectionString(builder.Configuration, serviceProvider));
        });
    }

    public static void ConfigureCustomServices(this WebApplicationBuilder builder)
    {
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

        // Infrastructure — Background Services
        builder.Services.AddHostedService<AutomationWorker>();
    }

    public static IAllowedOriginCache ConfigureCors(this WebApplicationBuilder builder)
    {
        // DB-backed, admin-editable allow-list (PREP.1). The same singleton instance is
        // registered for DI (so AllowedOrigins command handlers can refresh it after a
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

        return allowedOriginCache;
    }

    public static JwtSettings ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        // JWT Bearer, signed with the same key AUTH01.1's login endpoint mints tokens with.
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

        // Every endpoint requires an authenticated principal by default; [AllowAnonymous] opts
        // individual endpoints out (currently only the login endpoint itself).
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return jwtSettings;
    }

    public static void ConfigureGrpc(this WebApplicationBuilder builder)
    {
        // gRPC (MCP01) — second, internal-only transport for TaskManager.Mcp (MCP03), plain
        // HTTP/2 (no TLS — cleartext h2c, same direct/internal posture as the existing REST
        // port). Services are mapped in WebApplicationExtensions.ConfigureEndpoints; this port
        // isn't referenced anywhere in docker/nginx.conf, so it's never proxied/publicly reachable.
        //
        // Calling ListenAnyIP inside ConfigureKestrel disables ASPNETCORE_URLS-based endpoint
        // binding entirely (Kestrel logs "Overriding address(es)... Binding to endpoints
        // defined via IConfiguration and/or UseKestrel() instead" and drops the REST endpoint
        // otherwise) — verified live — so the existing REST URL(s) are re-declared here
        // alongside the new gRPC-only endpoint rather than left to the default URL-based binding.
        const int GrpcPort = 8082;
        var restUrls = builder.Configuration["ASPNETCORE_URLS"]
            ?? builder.Configuration["urls"]
            ?? "http://localhost:5062";

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            foreach (var rawUrl in restUrls.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var uri = new Uri(rawUrl.Replace("://+", "://0.0.0.0"));
                serverOptions.ListenAnyIP(uri.Port, listenOptions =>
                {
                    if (uri.Scheme == Uri.UriSchemeHttps)
                        listenOptions.UseHttps();
                });
            }

            serverOptions.ListenAnyIP(GrpcPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
        });

        builder.Services.AddGrpc();
    }

    public static void ConfigureRateLimiting(this WebApplicationBuilder builder)
    {
        // In-process only (no Redis/external limiter — MISSION.md's no-cloud-dependencies
        // pillar). Scoped to ApiToken-authenticated callers only: a JWT (human) principal
        // never carries an apiTokenId claim, so it falls into the no-op partition below and
        // is never throttled by this policy.
        const int DefaultRateLimitPerMinute = 60;
        const string RateLimitMessage = "Rate limit exceeded for this API token.";

        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (GrpcRejectionResponse.IsGrpcRequest(context.HttpContext))
                {
                    GrpcRejectionResponse.WriteTrailersOnly(context.HttpContext, StatusCode.ResourceExhausted, RateLimitMessage);
                    return;
                }

                context.HttpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync($$"""{"error":"{{RateLimitMessage}}"}""", cancellationToken);
            };

            // Applied via endpoint metadata (RequireRateLimiting in ConfigureEndpoints) rather
            // than as a global limiter, so REST controllers and gRPC services opt in the same
            // explicit way.
            options.AddPolicy("PerApiToken", httpContext =>
            {
                var apiTokenId = httpContext.User.FindFirst("apiTokenId")?.Value;
                if (apiTokenId is null)
                    return RateLimitPartition.GetNoLimiter("no-api-token");

                var rateLimitClaim = httpContext.User.FindFirst("rateLimitPerMinute")?.Value;
                var permitLimit = int.TryParse(rateLimitClaim, out var claimed) ? claimed : DefaultRateLimitPerMinute;

                return RateLimitPartition.GetFixedWindowLimiter(apiTokenId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = permitLimit,
                    Window            = TimeSpan.FromMinutes(1),
                    QueueLimit        = 0,
                    AutoReplenishment = true,
                });
            });
        });
    }
}
