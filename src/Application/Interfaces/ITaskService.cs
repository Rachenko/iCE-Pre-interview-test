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
