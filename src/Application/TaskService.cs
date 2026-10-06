using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Application;

public interface ITaskService
{
    Task<IReadOnlyList<ProjectTask>> GetAllAsync(int? projectId, string? status, CancellationToken ct = default);
    Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateTaskRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class TaskService(ITaskRepository repo) : ITaskService
{
    public Task<IReadOnlyList<ProjectTask>> GetAllAsync(int? projectId, string? status, CancellationToken ct = default)
        => repo.GetAllAsync(projectId, status, ct);
    public Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default) => repo.GetByIdAsync(id, ct);
    public Task<int> CreateAsync(CreateTaskRequest req, CancellationToken ct = default) => repo.CreateAsync(req, ct);
    public Task<bool> UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default) => repo.UpdateAsync(id, req, ct);
    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => repo.DeleteAsync(id, ct);
}
