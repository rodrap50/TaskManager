namespace TaskManager.Domain.Entities;

/// <summary>
/// One user's 1–10 priority vote on a single <see cref="ProjectTask"/>. Feeds into the
/// task's computed weighted score alongside its manually-set <see cref="TaskPriority"/>
/// and the parent <see cref="Project.CriticalityScore"/>.
/// </summary>
/// <remarks>
/// Always constructed via <see cref="ProjectTask.CastVote"/>, never directly — the parent
/// aggregate owns votes and enforces the one-row-per-user invariant in-memory (mirrored
/// by a unique DB constraint on <c>(TaskId, UserId)</c>). Casting a second vote from the
/// same user updates the existing row via <see cref="UpdateValue"/> rather than inserting
/// a new one.
/// </remarks>
public sealed class TaskVote
{
    public Guid Id { get; private set; }

    public Guid TaskId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>
    /// The voter's priority rating for this task, on a 1 (lowest) to 10 (highest) scale.
    /// </summary>
    public int VoteValue { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public TaskVote(Guid taskId, Guid userId, int voteValue)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("TaskId must be a valid, non-empty GUID.", nameof(taskId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        if (voteValue is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(voteValue), voteValue, "VoteValue must be between 1 and 10.");

        Id        = Guid.NewGuid();
        TaskId    = taskId;
        UserId    = userId;
        VoteValue = voteValue;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private TaskVote() { }

    /// <summary>
    /// Updates this vote's value in place (a user re-voting on the same task).
    /// </summary>
    /// <param name="newVoteValue">Replacement rating, 1–10.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   Thrown when <paramref name="newVoteValue"/> is outside 1–10.
    /// </exception>
    public void UpdateValue(int newVoteValue)
    {
        if (newVoteValue is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(newVoteValue), newVoteValue, "VoteValue must be between 1 and 10.");

        VoteValue = newVoteValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
