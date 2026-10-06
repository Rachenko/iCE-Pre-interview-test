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
