using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using TaskManager.Mcp.Data;
using TaskManager.Mcp.GrpcClients;

var builder = WebApplication.CreateBuilder(args);

// Consumed here (MCP_DB_CONNECTION_STRING → McpDbContext, TASKMANAGER_GRPC_URL → the
// GrpcClients/ wrapper) — read via standard configuration, rather than each ticket
// introducing its own config-reading pattern.
// MCP_DB_CONNECTION_STRING's value is never logged (it carries a Postgres password).
var grpcUrl = builder.Configuration["TASKMANAGER_GRPC_URL"] ?? "http://localhost:8082";

// One channel shared by every hierarchy client (MCP03.3) — plain h2c, matching the
// API's internal-only gRPC port (MCP01), no TLS to configure on either side.
builder.Services.AddSingleton(_ => GrpcChannel.ForAddress(grpcUrl));
builder.Services.AddSingleton<ProjectsGrpcClient>();
builder.Services.AddSingleton<EpicsGrpcClient>();
builder.Services.AddSingleton<PhasesGrpcClient>();
builder.Services.AddSingleton<TasksGrpcClient>();

// A database distinct from the monolith's "taskmanager" — MCP tracking data is never
// FK-coupled to the monolith's schema (MCP03.2). Dev value lives in appsettings.Development.json.
var dbConnectionString = builder.Configuration["MCP_DB_CONNECTION_STRING"]
    ?? throw new InvalidOperationException("MCP_DB_CONNECTION_STRING is not configured.");

builder.Services.AddDbContext<McpDbContext>(options => options.UseNpgsql(dbConnectionString));

// Lets tool methods take an IHttpContextAccessor parameter to read the caller's
// x-api-token header (MCP04) — the SDK flows HttpContext through stateful HTTP-transport
// tool calls specifically to support this.
builder.Services.AddHttpContextAccessor();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.Logger.LogInformation("TaskManager.Mcp starting. GrpcUrl={GrpcUrl}", grpcUrl);

// Creates McpTracking if it doesn't exist yet, then applies pending migrations — same
// migrate-on-startup convention as TaskManager.API, so a fresh compose stack needs no
// manual `dotnet ef database update` (MCP07.2).
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<McpDbContext>().Database.MigrateAsync();

app.MapMcp();

await app.RunAsync();
