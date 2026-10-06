using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Application;

// ---- Services: business logic layer between controllers and repositories ----
public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class UserService(IUserRepository repo) : IUserService
{
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) => repo.GetAllAsync(ct);
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) => repo.GetByIdAsync(id, ct);
    public Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default) => repo.CreateAsync(req, ct);
    public Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default) => repo.UpdateAsync(id, req, ct);
    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => repo.DeleteAsync(id, ct);
}

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

public interface IReportService
{
    Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default);
}

public sealed class ReportService(IReportRepository repo) : IReportService
{
    public Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default)
        => repo.GetDepartmentProgressAsync(ct);
    public Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default)
        => repo.GetProjectSummaryAsync(ct);
}
