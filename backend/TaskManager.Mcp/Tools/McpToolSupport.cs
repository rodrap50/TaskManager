using Grpc.Core;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol;

namespace TaskManager.Mcp.Tools;

/// <summary>
/// Shared plumbing for every hierarchy tool (MCP04) — no auth logic of its own, just
/// forwards the caller's token as-is and translates gRPC failures into clean MCP errors.
/// Real enforcement (expiry, read-only, rate limit) lives entirely in TaskManager.API (MCP02).
/// </summary>
internal static class McpToolSupport
{
    private const string ApiTokenHeaderName = "x-api-token";

    /// <summary>Raw token from the caller's MCP request, or "" if absent — never validated here.</summary>
    public static string GetToken(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.Request.Headers[ApiTokenHeaderName].ToString() ?? "";

    public static async Task<T> Execute<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (RpcException ex)
        {
            throw new McpException(ex.StatusCode switch
            {
                StatusCode.Unauthenticated => "Authentication failed: invalid, revoked, or expired API token.",
                StatusCode.PermissionDenied => "Permission denied: this API token is read-only.",
                StatusCode.InvalidArgument => $"Invalid input: {ex.Status.Detail}",
                StatusCode.FailedPrecondition => ex.Status.Detail,
                StatusCode.NotFound => "Not found.",
                _ => "Request to TaskManager.API failed.",
            });
        }
    }
}
