using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Application;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateProjectRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateProjectRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class ProjectService(IProjectRepository repo) : IProjectService
{
    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default) => repo.GetAllAsync(ct);
    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) => repo.GetByIdAsync(id, ct);
    public Task<int> CreateAsync(CreateProjectRequest req, CancellationToken ct = default) => repo.CreateAsync(req, ct);
    public Task<bool> UpdateAsync(int id, UpdateProjectRequest req, CancellationToken ct = default) => repo.UpdateAsync(id, req, ct);
    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => repo.DeleteAsync(id, ct);
}
