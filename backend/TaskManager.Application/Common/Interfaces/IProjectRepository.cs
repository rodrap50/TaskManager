using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetWithChildrenAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetAllWithChildrenAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads a project with its <see cref="Project.Members"/> collection tracked, for
    /// callers that need to mutate membership via <see cref="Project.AddMember"/> or
    /// <see cref="Project.RemoveMember"/> and save the result.
    /// </summary>
    Task<Project?> GetWithMembersAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Checks whether the given user is a member of the given project, without loading
    /// the full <see cref="Project.Members"/> collection.
    /// </summary>
    Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default);

    /// <summary>Every project's <see cref="Project.Key"/>, for picking a new unique one.</summary>
    Task<IReadOnlySet<string>> GetKeysAsync(CancellationToken ct = default);

    /// <summary>
    /// Takes the project's next task number and advances <see cref="Project.NextTaskNumber"/>
    /// in one atomic database statement, so concurrent creates never get the same number.
    /// It runs before the task insert, so a create that then fails leaves a gap; numbers are
    /// never reused.
    /// </summary>
    Task<int> AllocateTaskNumberAsync(Guid projectId, CancellationToken ct = default);
}
