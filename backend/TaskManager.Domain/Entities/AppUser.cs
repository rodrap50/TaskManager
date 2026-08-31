using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a registered user within the Task Manager system.
/// </summary>
/// <remarks>
/// <para>
/// <c>AppUser</c> is an aggregate root. It is intentionally framework-agnostic — no
/// ASP.NET Core Identity base classes, no EF Core data annotations. The Infrastructure
/// layer (B02) is responsible for all persistence mapping via Fluent API.
/// </para>
/// <para>
/// <b>Design decisions:</b>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Username"/> is stored in a normalized (lowercased) form to prevent
///       duplicate registrations that differ only by casing. The display-friendly form
///       is preserved separately in <see cref="DisplayName"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="PasswordHash"/> stores a bcrypt/PBKDF2 hash, never a plain-text
///       credential. Hashing is performed in the Application service layer (B04) before
///       this entity is ever constructed.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="AvatarUrl"/> is optional. When present it should point to a locally
///       hosted image path (e.g., <c>/avatars/{id}.webp</c>) to preserve data sovereignty.
///     </description>
///   </item>
///   <item>
///     <description>
///       The <see cref="AssignedTasks"/> collection is the inverse-navigation side of the
///       User ↔ ProjectTask relationship. It is initialised to an empty list to avoid
///       null-reference exceptions in LINQ operations prior to database hydration.
///     </description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public sealed class AppUser
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique surrogate primary key. Generated once at construction; never mutated.
    /// </summary>
    public Guid Id { get; private set; }

    // -------------------------------------------------------------------------
    // Profile
    // -------------------------------------------------------------------------

    /// <summary>
    /// Normalized (lowercase) unique username used for login and @-mentions.
    /// Must be between 3 and 50 characters and contain only alphanumeric characters,
    /// underscores, or hyphens.
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Human-readable display name shown in the UI (e.g., "Adam Smith").
    /// Not required to be unique. Maximum 100 characters.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// The user's email address. Used for notifications and account recovery.
    /// Stored in lowercase-normalized form. Must be unique across all users.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// A pre-computed, salted credential hash (e.g., PBKDF2 or bcrypt).
    /// The Application layer is solely responsible for hashing before passing
    /// values to this entity. This property is intentionally <c>private set</c>
    /// to enforce the mutation path through <see cref="UpdatePasswordHash"/>.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Optional relative or absolute URL pointing to the user's avatar image.
    /// Designed for local hosting (e.g., <c>/avatars/3f2a...webp</c>).
    /// <see langword="null"/> when no avatar has been uploaded.
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Indicates whether the user account is currently active and permitted to
    /// authenticate. Soft-disabling a user preserves all historical data.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indicates whether the user holds administrative privileges (e.g., user
    /// management, webhook configuration). Maps to the Admin panel described in Phase 4b.
    /// </summary>
    public bool IsAdmin { get; private set; }

    // -------------------------------------------------------------------------
    // Audit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all temporal and optimistic-concurrency metadata.
    /// See <see cref="AuditInfo"/> for the concurrency-token contract.
    /// </summary>
    public AuditInfo Audit { get; private set; }

    // -------------------------------------------------------------------------
    // Navigation (inverse side)
    // -------------------------------------------------------------------------

    /// <summary>
    /// The collection of tasks currently or historically assigned to this user.
    /// This is the inverse navigation side of the <c>AppUser ↔ ProjectTask</c>
    /// one-to-many relationship; ownership lives on <see cref="ProjectTask"/>.
    /// </summary>
    public IReadOnlyCollection<ProjectTask> AssignedTasks => _assignedTasks.AsReadOnly();

    private readonly List<ProjectTask> _assignedTasks;

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new, active <see cref="AppUser"/>.
    /// </summary>
    /// <param name="username">
    ///   Will be stored in lowercase-normalized form. Must be 3–50 characters,
    ///   alphanumeric, underscores, or hyphens only.
    /// </param>
    /// <param name="displayName">Human-friendly name shown in the UI.</param>
    /// <param name="email">Lowercase-normalized unique email address.</param>
    /// <param name="passwordHash">
    ///   Pre-computed credential hash. The caller (Application service) is
    ///   responsible for hashing before invoking this constructor.
    /// </param>
    /// <param name="isAdmin">
    ///   <see langword="true"/> if the user should receive administrative privileges
    ///   on account creation. Defaults to <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="username"/>, <paramref name="displayName"/>,
    ///   <paramref name="email"/>, or <paramref name="passwordHash"/> is null, empty,
    ///   or whitespace.
    /// </exception>
    public AppUser(
        string username,
        string displayName,
        string email,
        string passwordHash,
        bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must not be null or whitespace.", nameof(username));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name must not be null or whitespace.", nameof(displayName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email must not be null or whitespace.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash must not be null or whitespace.", nameof(passwordHash));

        Id            = Guid.NewGuid();
        Username      = username.ToLowerInvariant().Trim();
        DisplayName   = displayName.Trim();
        Email         = email.ToLowerInvariant().Trim();
        PasswordHash  = passwordHash;
        IsActive      = true;
        IsAdmin       = isAdmin;
        Audit         = AuditInfo.CreateNew();
        _assignedTasks = [];
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private AppUser()
    {
        // EF Core materializes the entity via reflection; field initializers below
        // ensure the object is in a valid state prior to property hydration.
        Username      = string.Empty;
        DisplayName   = string.Empty;
        Email         = string.Empty;
        PasswordHash  = string.Empty;
        Audit         = AuditInfo.CreateNew();
        _assignedTasks = [];
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the user's display name and records the mutation timestamp.
    /// </summary>
    /// <param name="newDisplayName">Must not be null, empty, or whitespace.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newDisplayName"/> is null, empty, or whitespace.
    /// </exception>
    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Display name must not be null or whitespace.", nameof(newDisplayName));

        DisplayName = newDisplayName.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Replaces the stored credential hash with a new one.
    /// The Application layer must validate the current credential and compute the
    /// new hash before invoking this method.
    /// </summary>
    /// <param name="newPasswordHash">Pre-computed replacement hash.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newPasswordHash"/> is null, empty, or whitespace.
    /// </exception>
    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash must not be null or whitespace.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or clears the user's avatar URL.
    /// Pass <see langword="null"/> to remove an existing avatar.
    /// </summary>
    /// <param name="avatarUrl">
    ///   Relative or absolute URL, or <see langword="null"/> to clear.
    /// </param>
    public void UpdateAvatarUrl(string? avatarUrl)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Grants administrative privileges to this user account.
    /// </summary>
    public void PromoteToAdmin()
    {
        IsAdmin = true;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Revokes administrative privileges from this user account.
    /// </summary>
    public void RevokeAdmin()
    {
        IsAdmin = false;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Deactivates the account, preventing future authentication.
    /// All historical data is preserved. Use <see cref="Reactivate"/> to reverse.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Restores a previously deactivated account to an active state.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        Audit = Audit.Touch();
    }
}
