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

// Dev fallback points at a database distinct from the monolith's "taskmanager" — MCP
// tracking data is never FK-coupled to the monolith's schema (MCP03.2).
const string devFallbackConnectionString =
    "Host=localhost;Port=5432;Database=McpTracking;Username=postgres;Password=postgres";
var dbConnectionString = builder.Configuration["MCP_DB_CONNECTION_STRING"] ?? devFallbackConnectionString;

builder.Services.AddDbContext<McpDbContext>(options => options.UseNpgsql(dbConnectionString));

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.Logger.LogInformation(
    "TaskManager.Mcp starting. GrpcUrl={GrpcUrl}, DbConnectionConfigured={DbConnectionConfigured}",
    grpcUrl, !string.IsNullOrWhiteSpace(builder.Configuration["MCP_DB_CONNECTION_STRING"]));

app.MapMcp();

app.Run();
