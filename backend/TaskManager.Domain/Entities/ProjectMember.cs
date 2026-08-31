namespace TaskManager.Domain.Entities;

/// <summary>
/// Flat join record representing an <see cref="AppUser"/>'s membership in a <see cref="Project"/>.
/// No per-project role field — admin powers reuse the global <see cref="AppUser.IsAdmin"/>
/// flag; full RBAC is deferred post-MVP.
/// </summary>
/// <remarks>
/// Always constructed via <see cref="Project.AddMember"/>, never directly — the parent
/// aggregate owns membership and enforces the one-row-per-user invariant in-memory (mirrored
/// by a unique DB constraint on <c>(ProjectId, UserId)</c>).
/// </remarks>
public sealed class ProjectMember
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public ProjectMember(Guid projectId, Guid userId)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId must be a valid, non-empty GUID.", nameof(projectId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        Id        = Guid.NewGuid();
        ProjectId = projectId;
        UserId    = userId;
        JoinedAt  = DateTime.UtcNow;
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private ProjectMember() { }
}
