namespace TaskManager.Domain.ValueObjects;

/// <summary>
/// An immutable value object that bundles all temporal tracking and optimistic-concurrency
/// metadata for any domain aggregate root or entity.
/// </summary>
/// <remarks>
/// <para>
/// Implemented as a C# <c>record</c> to guarantee structural equality and immutability
/// by default. No two <c>AuditInfo</c> instances with identical field values are
/// distinguishable, which is the correct semantic for a value object.
/// </para>
/// <para>
/// <b>Concurrency token (<see cref="RowVersion"/>):</b> An ever-increasing <see langword="long"/>
/// derived from <see cref="DateTime.UtcNow"/> ticks. EF Core (B02) will map this to a
/// <c>rowversion</c>/<c>xmin</c> column and use it for optimistic-concurrency conflict
/// detection. Offline clients store and echo this value; the server rejects writes where
/// the incoming token does not match the current persisted token.
/// </para>
/// </remarks>
/// <param name="CreatedAt">
///   UTC timestamp recorded once, at entity construction. Never mutated after creation.
/// </param>
/// <param name="UpdatedAt">
///   UTC timestamp of the most recent mutation. <see langword="null"/> until the first
///   post-creation edit is applied.
/// </param>
/// <param name="RowVersion">
///   Monotonically increasing concurrency token. Defaults to <see cref="DateTime.UtcNow"/>
///   ticks at construction time and must be incremented by the persistence layer on every
///   successful write.
/// </param>
public sealed record AuditInfo(
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    long RowVersion)
{
    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a brand-new <see cref="AuditInfo"/> stamped with the current UTC clock.
    /// Use this factory when constructing a new aggregate root for the first time.
    /// </summary>
    public static AuditInfo CreateNew()
    {
        var utcNow = DateTime.UtcNow;
        return new AuditInfo(
            CreatedAt: utcNow,
            UpdatedAt: null,
            RowVersion: utcNow.Ticks);
    }

    // -------------------------------------------------------------------------
    // Mutation (produces a new instance — immutability preserved)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a new <see cref="AuditInfo"/> with <see cref="UpdatedAt"/> set to the
    /// current UTC clock and <see cref="RowVersion"/> incremented.
    /// <see cref="CreatedAt"/> is never changed.
    /// </summary>
    public AuditInfo Touch()
    {
        var utcNow = DateTime.UtcNow;
        return this with
        {
            UpdatedAt = utcNow,
            RowVersion = utcNow.Ticks
        };
    }
}
