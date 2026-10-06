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

// Repository and service interfaces live in the Interfaces/ folder.
