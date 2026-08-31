namespace TaskManager.Application.Common.Exceptions;

/// <summary>
/// Thrown by a command/query handler when the current authenticated user is not permitted
/// to perform the requested action (e.g. voting on a task in a project they don't belong
/// to). Mapped to HTTP 403 by the global exception handler in <c>Program.cs</c> — distinct
/// from FluentValidation's <see cref="FluentValidation.ValidationException"/>, which maps to
/// 400 for malformed input rather than an authorization failure.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message) { }
}
