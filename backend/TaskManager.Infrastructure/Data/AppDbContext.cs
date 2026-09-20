using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<ProjectPhase> Phases => Set<ProjectPhase>();
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<ApiToken> ApiTokens => Set<ApiToken>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskVote> TaskVotes => Set<TaskVote>();
    public DbSet<AllowedOrigin> AllowedOrigins => Set<AllowedOrigin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureAppUser(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureProjectPhase(modelBuilder);
        ConfigureEpic(modelBuilder);
        ConfigureProjectTask(modelBuilder);
        ConfigureApiToken(modelBuilder);
        ConfigureProjectMember(modelBuilder);
        ConfigureTaskVote(modelBuilder);
        ConfigureAllowedOrigin(modelBuilder);
    }

    // -------------------------------------------------------------------------
    // Entity configurations
    // -------------------------------------------------------------------------

    private static void ConfigureAppUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(b =>
        {
            b.HasKey(u => u.Id);

            b.Property(u => u.Username).HasMaxLength(50).IsRequired();
            b.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
            b.Property(u => u.Email).HasMaxLength(254).IsRequired();
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.AvatarUrl).HasMaxLength(500);

            b.HasIndex(u => u.Username).IsUnique();
            b.HasIndex(u => u.Email).IsUnique();

            // Enforces "at most one admin exists" at the DB level — the actual correctness
            // guarantee behind AUTH02.3's setup endpoint being race-safe under concurrent
            // double-submission. The application-level existence check in
            // SetupAdminCommandHandler is only a fast-path; this index is what a concurrent
            // insert actually collides against.
            b.HasIndex(u => u.IsAdmin)
                .IsUnique()
                .HasFilter("\"IsAdmin\" = true")
                .HasDatabaseName("IX_Users_SingleAdmin");

            b.OwnsOne(u => u.Audit, audit =>
            {
                audit.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired();
                audit.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                audit.Property(a => a.RowVersion).HasColumnName("RowVersion").IsConcurrencyToken();
            });
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(b =>
        {
            b.HasKey(p => p.Id);

            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Description).HasMaxLength(2000);
            b.Property(p => p.ColorHex).HasMaxLength(6);

            // Matches Project's own in-memory default (DefaultCriticalityScore) so existing
            // rows backfilled by the AddTaskVoteAndCriticalityScore migration land on the same
            // neutral midpoint as a newly-constructed Project, rather than an invalid 0.
            b.Property(p => p.CriticalityScore).HasDefaultValue(Project.DefaultCriticalityScore);

            b.OwnsOne(p => p.Audit, audit =>
            {
                audit.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired();
                audit.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                audit.Property(a => a.RowVersion).HasColumnName("RowVersion").IsConcurrencyToken();
            });

            // CreatedByUserId is non-nullable so Restrict prevents orphaning projects on user delete.
            b.HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(p => p.Phases)
                .WithOne(ph => ph.Project)
                .HasForeignKey(ph => ph.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Epics)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Members)
                .WithOne()
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectPhase(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectPhase>(b =>
        {
            b.HasKey(ph => ph.Id);

            b.Property(ph => ph.Name).HasMaxLength(100).IsRequired();
            b.Property(ph => ph.Description).HasMaxLength(500);

            b.OwnsOne(ph => ph.Audit, audit =>
            {
                audit.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired();
                audit.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                audit.Property(a => a.RowVersion).HasColumnName("RowVersion").IsConcurrencyToken();
            });

            // SetNull: deleting a phase unassigns its tasks rather than deleting them.
            b.HasMany(ph => ph.Tasks)
                .WithOne(t => t.Phase)
                .HasForeignKey(t => t.PhaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureEpic(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Epic>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.Name).HasMaxLength(100).IsRequired();
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.ColorHex).HasMaxLength(6);

            b.OwnsOne(e => e.Audit, audit =>
            {
                audit.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired();
                audit.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                audit.Property(a => a.RowVersion).HasColumnName("RowVersion").IsConcurrencyToken();
            });

            // SetNull: deleting an epic clears the association on its tasks rather than deleting them.
            b.HasMany(e => e.Tasks)
                .WithOne(t => t.Epic)
                .HasForeignKey(t => t.EpicId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureProjectTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectTask>(b =>
        {
            b.HasKey(t => t.Id);

            b.Property(t => t.Title).HasMaxLength(300).IsRequired();
            b.Property(t => t.Description).HasMaxLength(4000);
            b.Property(t => t.EstimatedHours).HasPrecision(6, 2);

            // Dictionary<string, string> stored as PostgreSQL JSONB for flexible external
            // tool metadata (Home Assistant entity IDs, MQTT topics, webhook references).
            // The value comparer is required so EF Core can detect dictionary mutations.
            var metadataComparer = new ValueComparer<Dictionary<string, string>>(
                (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null)
                       == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, string>>(
                         JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                         (JsonSerializerOptions?)null)
                     ?? new Dictionary<string, string>());

            b.Property(t => t.ExternalMetadata)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                         ?? new Dictionary<string, string>())
                .Metadata.SetValueComparer(metadataComparer);

            b.OwnsOne(t => t.Audit, audit =>
            {
                audit.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired();
                audit.Property(a => a.UpdatedAt).HasColumnName("UpdatedAt");
                audit.Property(a => a.RowVersion).HasColumnName("RowVersion").IsConcurrencyToken();
            });

            // Primary assignee — inverse collection lives on AppUser.AssignedTasks.
            b.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Secondary assignee — no inverse collection needed.
            b.HasOne(t => t.SecondaryAssignee)
                .WithMany()
                .HasForeignKey(t => t.SecondaryAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cascade: deleting a task deletes its votes too (unlike Phase/Epic, which
            // SetNull on their tasks — a vote has no meaning detached from its task).
            b.HasMany(t => t.Votes)
                .WithOne()
                .HasForeignKey(v => v.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureApiToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiToken>(b =>
        {
            b.HasKey(t => t.Id);

            b.Property(t => t.Name).HasMaxLength(100).IsRequired();
            b.Property(t => t.TokenHash).IsRequired();
            b.Property(t => t.IsReadOnly).IsRequired();
            b.Property(t => t.ExpiresAt);
            b.Property(t => t.RateLimitPerMinute);

            // No navigation property either side — CreatedByUserId is informational only.
            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAllowedOrigin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllowedOrigin>(b =>
        {
            b.HasKey(o => o.Id);

            b.Property(o => o.OriginUrl).HasMaxLength(500).IsRequired();

            b.HasIndex(o => o.OriginUrl).IsUnique();
        });
    }

    private static void ConfigureProjectMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectMember>(b =>
        {
            b.HasKey(m => m.Id);

            // ProjectMember is the only entity in this app added via a collection-navigation
            // fixup (Project.AddMember) on an ALREADY-TRACKED parent, rather than an explicit
            // repository .Add() call. By EF Core's default Guid-key convention (ValueGeneratedOnAdd),
            // a non-default key value set before tracking begins reads as "this already exists",
            // so DetectChanges marks it Modified instead of Added — producing a no-op UPDATE
            // against a row that was never inserted. ValueGeneratedNever() (correct here, since
            // the key really is always client-generated via Guid.NewGuid()) fixes the detection.
            b.Property(m => m.Id).ValueGeneratedNever();

            b.HasIndex(m => new { m.ProjectId, m.UserId }).IsUnique();

            // No navigation property either side — matches ApiToken.CreatedByUserId's style.
            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTaskVote(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskVote>(b =>
        {
            b.HasKey(v => v.Id);

            // Same fixup pattern as ProjectMember (added via ProjectTask.CastVote on an
            // already-tracked parent) — ValueGeneratedNever() is required for the same
            // reason documented on ProjectMember.Id above.
            b.Property(v => v.Id).ValueGeneratedNever();

            b.HasIndex(v => new { v.TaskId, v.UserId }).IsUnique();

            // No navigation property either side — matches ProjectMember.UserId's style.
            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
