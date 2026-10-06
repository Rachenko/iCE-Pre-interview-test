using Microsoft.EntityFrameworkCore;
using ProjectMonitoring.Application;
using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Infrastructure;

public sealed class UserRepository(MonitoringDbContext db) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            FullName = req.FullName,
            Department = req.Department,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is null) return false;
        user.Email = req.Email;
        user.FullName = req.FullName;
        user.Department = req.Department;
        return await db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is null) return false;
        db.Users.Remove(user);
        return await db.SaveChangesAsync(ct) > 0;
    }
}

public sealed class ProjectRepository(MonitoringDbContext db) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default) =>
        await db.Projects.AsNoTracking().OrderBy(p => p.Id).ToListAsync(ct);

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<int> CreateAsync(CreateProjectRequest req, CancellationToken ct = default)
    {
        var project = new Project
        {
            Name = req.Name,
            Description = req.Description,
            OwnerId = req.OwnerId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return project.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProjectRequest req, CancellationToken ct = default)
    {
        var project = await db.Projects.FindAsync([id], ct);
        if (project is null) return false;
        project.Name = req.Name;
        project.Description = req.Description;
        project.Status = req.Status;
        project.EndDate = req.EndDate;
        return await db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await db.Projects.FindAsync([id], ct);
        if (project is null) return false;
        db.Projects.Remove(project);
        return await db.SaveChangesAsync(ct) > 0;
    }
}

public sealed class TaskRepository(MonitoringDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<ProjectTask>> GetAllAsync(int? projectId, string? status, CancellationToken ct = default) =>
        await db.Tasks.AsNoTracking()
            .Where(t => projectId == null || t.ProjectId == projectId)
            .Where(t => status == null || t.Status == status)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);

    public async Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<int> CreateAsync(CreateTaskRequest req, CancellationToken ct = default)
    {
        var task = new ProjectTask
        {
            ProjectId = req.ProjectId,
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            AssigneeId = req.AssigneeId,
            DueDate = req.DueDate,
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest req, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return false;
        task.Title = req.Title;
        task.Description = req.Description;
        task.Priority = req.Priority;
        task.AssigneeId = req.AssigneeId;
        task.DueDate = req.DueDate;
        if (req.Status == "done" && task.Status != "done")
            task.CompletedAt = DateTime.UtcNow;
        else if (req.Status != "done")
            task.CompletedAt = null;
        task.Status = req.Status;
        return await db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return false;
        db.Tasks.Remove(task);
        return await db.SaveChangesAsync(ct) > 0;
    }
}

public sealed class ReportRepository(MonitoringDbContext db) : IReportRepository
{
    public async Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default)
    {
        var rows = await db.Users
            .Join(db.Tasks, u => u.Id, t => t.AssigneeId, (u, t) => new { u.Department, t.Status })
            .GroupBy(x => x.Department)
            .Select(g => new
            {
                Department = g.Key,
                TotalTasks = g.LongCount(),
                DoneTasks = g.LongCount(x => x.Status == "done"),
                InProgressTasks = g.LongCount(x => x.Status == "in_progress"),
                BlockedTasks = g.LongCount(x => x.Status == "blocked"),
            })
            .OrderBy(r => r.Department)
            .ToListAsync(ct);

        return rows.Select(r => new DepartmentProgressReport(
            r.Department,
            r.TotalTasks,
            r.DoneTasks,
            r.InProgressTasks,
            r.BlockedTasks,
            Math.Round(100m * r.DoneTasks / r.TotalTasks, 2))).ToList();
    }

    public async Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default)
    {
        var rows = await db.Projects
            .Join(db.Users, p => p.OwnerId, u => u.Id, (p, u) => new { p, u })
            .GroupJoin(db.Tasks, x => x.p.Id, t => t.ProjectId, (x, ts) => new { x.p, x.u, ts })
            .Select(x => new
            {
                x.p.Id,
                x.p.Name,
                x.p.Status,
                OwnerName = x.u.FullName,
                x.u.Department,
                TotalTasks = x.ts.LongCount(),
                DoneTasks = x.ts.LongCount(t => t.Status == "done"),
                NearestDueDate = x.ts.Where(t => t.Status != "done").Min(t => t.DueDate),
            })
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

        return rows.Select(r => new ProjectSummaryReport(
            r.Id,
            r.Name,
            r.Status,
            r.OwnerName,
            r.Department,
            r.TotalTasks,
            r.DoneTasks,
            r.TotalTasks == 0 ? null : Math.Round(100m * r.DoneTasks / r.TotalTasks, 2),
            r.NearestDueDate?.ToDateTime(TimeOnly.MinValue))).ToList();
    }
}
