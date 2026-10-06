using Dapper;
using Npgsql;
using ProjectMonitoring.Application;
using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Infrastructure;

public sealed class UserRepository(NpgsqlDataSource db) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<User>(
            "SELECT id, username, email, full_name AS FullName, department, created_at AS CreatedAt FROM users ORDER BY id");
        return rows.AsList();
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT id, username, email, full_name AS FullName, department, created_at AS CreatedAt FROM users WHERE id = @id",
            new { id });
    }

    public async Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO users (username, email, full_name, department) VALUES (@Username, @Email, @FullName, @Department) RETURNING id",
            req);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var n = await conn.ExecuteAsync(
            "UPDATE users SET email = @Email, full_name = @FullName, department = @Department WHERE id = @id",
            new { id, req.Email, req.FullName, req.Department });
        return n > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteAsync("DELETE FROM users WHERE id = @id", new { id }) > 0;
    }
}

public sealed class ProjectRepository(NpgsqlDataSource db) : IProjectRepository
{
    private const string Select =
        "SELECT id, name, description, status, owner_id AS OwnerId, start_date AS StartDate, end_date AS EndDate, created_at AS CreatedAt FROM projects";

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return (await conn.QueryAsync<Project>(Select + " ORDER BY id")).AsList();
    }

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<Project>(Select + " WHERE id = @id", new { id });
    }

    public async Task<int> CreateAsync(CreateProjectRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO projects (name, description, owner_id, start_date, end_date) VALUES (@Name, @Description, @OwnerId, @StartDate, @EndDate) RETURNING id",
            req);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProjectRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var n = await conn.ExecuteAsync(
            "UPDATE projects SET name = @Name, description = @Description, status = @Status, end_date = @EndDate WHERE id = @id",
            new { id, req.Name, req.Description, req.Status, req.EndDate });
        return n > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteAsync("DELETE FROM projects WHERE id = @id", new { id }) > 0;
    }
}

public sealed class TaskRepository(NpgsqlDataSource db) : ITaskRepository
{
    private const string Select =
        "SELECT id, project_id AS ProjectId, title, description, status, priority, " +
        "assignee_id AS AssigneeId, due_date AS DueDate, completed_at AS CompletedAt, " +
        "created_at AS CreatedAt FROM tasks";

    public async Task<IReadOnlyList<ProjectTask>> GetAllAsync(int? projectId, string? status, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var sql = Select + " WHERE (@projectId IS NULL OR project_id = @projectId) AND (@status IS NULL OR status = @status) ORDER BY id";
        return (await conn.QueryAsync<ProjectTask>(sql, new { projectId, status })).AsList();
    }

    public async Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<ProjectTask>(Select + " WHERE id = @id", new { id });
    }

    public async Task<int> CreateAsync(CreateTaskRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO tasks (project_id, title, description, priority, assignee_id, due_date) " +
            "VALUES (@ProjectId, @Title, @Description, @Priority, @AssigneeId, @DueDate) RETURNING id",
            req);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var n = await conn.ExecuteAsync(
            "UPDATE tasks SET title = @Title, description = @Description, status = @Status, " +
            "priority = @Priority, assignee_id = @AssigneeId, due_date = @DueDate, " +
            "completed_at = CASE WHEN @Status = 'done' AND completed_at IS NULL THEN now() " +
            "WHEN @Status <> 'done' THEN NULL ELSE completed_at END WHERE id = @id",
            new { id, req.Title, req.Description, req.Status, req.Priority, req.AssigneeId, req.DueDate });
        return n > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        return await conn.ExecuteAsync("DELETE FROM tasks WHERE id = @id", new { id }) > 0;
    }
}

public sealed class ReportRepository(NpgsqlDataSource db) : IReportRepository
{
    public async Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var sql = """
            SELECT u.department AS Department,
                   COUNT(t.id)                                                          AS TotalTasks,
                   COUNT(t.id) FILTER (WHERE t.status = 'done')                         AS DoneTasks,
                   COUNT(t.id) FILTER (WHERE t.status = 'in_progress')                  AS InProgressTasks,
                   COUNT(t.id) FILTER (WHERE t.status = 'blocked')                      AS BlockedTasks,
                   ROUND(100.0 * COUNT(t.id) FILTER (WHERE t.status = 'done')
                         / NULLIF(COUNT(t.id), 0), 2)                                   AS ProgressPercent
            FROM users u
            JOIN tasks t ON t.assignee_id = u.id
            GROUP BY u.department
            ORDER BY u.department
            """;
        return (await conn.QueryAsync<DepartmentProgressReport>(sql)).AsList();
    }

    public async Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        var sql = """
            SELECT p.id              AS ProjectId,
                   p.name            AS ProjectName,
                   p.status          AS Status,
                   u.full_name       AS OwnerName,
                   u.department      AS Department,
                   COUNT(t.id)                                                  AS TotalTasks,
                   COUNT(t.id) FILTER (WHERE t.status = 'done')                 AS DoneTasks,
                   ROUND(100.0 * COUNT(t.id) FILTER (WHERE t.status = 'done')
                         / NULLIF(COUNT(t.id), 0), 2)                           AS ProgressPercent,
                   MIN(t.due_date) FILTER (WHERE t.status <> 'done')            AS NearestDueDate
            FROM projects p
            JOIN users u      ON u.id = p.owner_id
            LEFT JOIN tasks t ON t.project_id = p.id
            GROUP BY p.id, p.name, p.status, u.full_name, u.department
            ORDER BY p.id
            """;
        return (await conn.QueryAsync<ProjectSummaryReport>(sql)).AsList();
    }
}
