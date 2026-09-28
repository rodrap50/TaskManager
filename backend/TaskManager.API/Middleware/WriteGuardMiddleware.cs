using Grpc.Core;

namespace TaskManager.API.Middleware;

/// <summary>
/// Blocks non-safe REST requests (POST/PUT/PATCH/DELETE) and mutating gRPC RPCs from an
/// ApiToken-authenticated principal carrying isReadOnly=true (MCP02.2). JWT-authenticated
/// (human) requests are unaffected — only ApiToken auth ever sets the isReadOnly claim.
/// </summary>
/// <remarks>
/// Deliberately a plain ASP.NET Core pipeline middleware, not an MVC action filter — filters
/// never run for gRPC endpoints, and this needs to cover both transports identically. gRPC
/// carries no distinct HTTP verb (every RPC is a POST at the transport level), so mutating
/// RPCs are identified by name instead, against the fixed set MCP01.3/MCP01.4 actually shipped.
/// </remarks>
public sealed class WriteGuardMiddleware
{
    private static readonly HashSet<string> SafeHttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get, HttpMethods.Head, HttpMethods.Options,
    };

    private static readonly HashSet<string> MutatingGrpcMethods = new(StringComparer.Ordinal)
    {
        "CreateEpic", "UpdateEpic",
        "CreateTask", "UpdateTask", "TransitionTask",
    };

    private const string ReadOnlyMessage = "This token is read-only and cannot perform mutating operations.";

    private readonly RequestDelegate _next;

    public WriteGuardMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.FindFirst("isReadOnly")?.Value != "true")
        {
            await _next(context);
            return;
        }

        if (GrpcRejectionResponse.IsGrpcRequest(context))
        {
            var rpcMethod = context.Request.Path.Value?.Split('/').LastOrDefault();
            if (rpcMethod is not null && MutatingGrpcMethods.Contains(rpcMethod))
            {
                GrpcRejectionResponse.WriteTrailersOnly(context, StatusCode.PermissionDenied, ReadOnlyMessage);
                return;
            }
        }
        else if (!SafeHttpMethods.Contains(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($$"""{"error":"{{ReadOnlyMessage}}"}""");
            return;
        }

        await _next(context);
    }
}
