using Grpc.Core;

namespace TaskManager.API.Middleware;

/// <summary>
/// Shared helper for rejecting a gRPC call from generic (non-gRPC-aware) ASP.NET Core
/// middleware — used by both <see cref="WriteGuardMiddleware"/> (MCP02.3) and the rate
/// limiter's rejection handler (MCP02.4), so a call fails with a real gRPC status instead of
/// a bare HTTP error grpc-dotnet's client can only guess at.
/// </summary>
internal static class GrpcRejectionResponse
{
    public static bool IsGrpcRequest(HttpContext context) =>
        context.Request.ContentType?.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Writes a "Trailers-Only" gRPC rejection — the status lives in trailers, not the HTTP
    /// status line, so the HTTP status itself stays 200 and no response body is ever written.
    /// </summary>
    public static void WriteTrailersOnly(HttpContext context, StatusCode grpcStatus, string message)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/grpc";
        context.Response.AppendTrailer("grpc-status", ((int)grpcStatus).ToString());
        context.Response.AppendTrailer("grpc-message", message);
    }
}
