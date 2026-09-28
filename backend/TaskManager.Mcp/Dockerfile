# syntax=docker/dockerfile:1
#
# Build context must be backend/ (the parent of this project and its sibling
# TaskManager.Grpc.Contracts):
#   docker build -f backend/TaskManager.Mcp/Dockerfile -t taskmanager-mcp backend/

# ---------------------------------------------------------------------------
# Stage: build — publish TaskManager.Mcp (Release)
#
# Debian-based SDK image, not -alpine: TaskManager.Grpc.Contracts' protoc
# codegen needs Grpc.Tools' bundled native protoc binary, which only ships
# glibc-linked Linux builds (no linux_musl_x64) — it can't run under Alpine.
# Only this build stage needs glibc; the final runtime stage below is still
# Alpine since it only runs already-compiled DLLs.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskManager.Mcp/TaskManager.Mcp.csproj TaskManager.Mcp/
COPY TaskManager.Grpc.Contracts/TaskManager.Grpc.Contracts.csproj TaskManager.Grpc.Contracts/
RUN dotnet restore "TaskManager.Mcp/TaskManager.Mcp.csproj"

COPY TaskManager.Mcp/ TaskManager.Mcp/
COPY TaskManager.Grpc.Contracts/ TaskManager.Grpc.Contracts/
WORKDIR /src/TaskManager.Mcp
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage: final — ASP.NET Core runtime, non-root (same appuser convention as
# the monolithic image's D02 final stage)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S -g 1000 appuser \
 && adduser -S -G appuser -u 1000 -h /app appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
# TASKMANAGER_GRPC_URL / MCP_DB_CONNECTION_STRING have no default here — both
# are environment-specific (the monolith's internal gRPC port, and the
# McpTracking connection string) and are set at `docker run`/compose time
# (MCP07.2).

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.Mcp.dll"]
