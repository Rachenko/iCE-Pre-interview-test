using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Application;

// ---- DTOs ----
public record CreateUserRequest(string Username, string Email, string FullName, string Department);
public record UpdateUserRequest(string Email, string FullName, string Department);

public record CreateProjectRequest(string Name, string? Description, int OwnerId, DateOnly StartDate, DateOnly? EndDate);
public record UpdateProjectRequest(string Name, string? Description, string Status, DateOnly? EndDate);

public record CreateTaskRequest(int ProjectId, string Title, string? Description, string Priority, int? AssigneeId, DateOnly? DueDate);
public record UpdateTaskRequest(string Title, string? Description, string Status, string Priority, int? AssigneeId, DateOnly? DueDate);

public record DepartmentProgressReport(
    string Department,
    long TotalTasks,
    long DoneTasks,
    long InProgressTasks,
    long BlockedTasks,
    decimal ProgressPercent);

public record ProjectSummaryReport(
    int ProjectId,
    string ProjectName,
    string Status,
    string OwnerName,
    string Department,
    long TotalTasks,
    long DoneTasks,
    decimal? ProgressPercent,
    DateTime? NearestDueDate);

// ---- Repository interfaces (ports) ----
public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateProjectRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateProjectRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface ITaskRepository
{
    Task<IReadOnlyList<ProjectTask>> GetAllAsync(int? projectId, string? status, CancellationToken ct = default);
    Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateTaskRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IReportRepository
{
    Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default);
}
