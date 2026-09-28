using Grpc.Core;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

/// <summary>
/// Shared plumbing for every tool — no auth logic of its own, just forwards the caller's
/// token as-is and translates gRPC failures into clean MCP errors. Real enforcement
/// (expiry, read-only, rate limit) lives entirely in TaskManager.API (MCP02).
/// </summary>
internal static class McpToolSupport
{
    private const string ApiTokenHeaderName = "x-api-token";

    /// <summary>
    /// For tools that never otherwise reach the API (agent-plan tools, MCP08): asks the API
    /// whether the caller's token is valid, then refuses writes from a read-only token.
    /// </summary>
    public static async Task RequireToken(AuthGrpcClient auth, IHttpContextAccessor httpContextAccessor, bool write)
    {
        var result = await Execute(() => auth.ValidateToken(GetToken(httpContextAccessor)));
        if (write && result.IsReadOnly)
            throw new McpException("Permission denied: this API token is read-only.");
    }

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
