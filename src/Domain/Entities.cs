namespace ProjectMonitoring.Domain;

public enum ProjectStatus { Active, OnHold, Completed, Cancelled }
public enum TaskStatus { Todo, InProgress, Done, Blocked }
public enum TaskPriority { Low, Medium, High, Critical }

public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "active";
    public int OwnerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ProjectTask
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "todo";
    public string Priority { get; set; } = "medium";
    public int? AssigneeId { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SystemLog
{
    public long Id { get; set; }
    public string Level { get; set; } = "info";
    public string Source { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
