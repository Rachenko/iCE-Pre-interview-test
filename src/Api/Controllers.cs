using System.Text;
using Microsoft.AspNetCore.Mvc;
using ProjectMonitoring.Application;

namespace ProjectMonitoring.Api;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IUserService svc) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await svc.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await svc.GetByIdAsync(id, ct) is { } u ? Ok(u) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req, CancellationToken ct)
    {
        var id = await svc.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest req, CancellationToken ct)
        => await svc.UpdateAsync(id, req, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await svc.DeleteAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(IProjectService svc) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await svc.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await svc.GetByIdAsync(id, ct) is { } p ? Ok(p) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest req, CancellationToken ct)
    {
        var id = await svc.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProjectRequest req, CancellationToken ct)
        => await svc.UpdateAsync(id, req, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await svc.DeleteAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? projectId, [FromQuery] string? status, CancellationToken ct)
        => Ok(await svc.GetAllAsync(projectId, status, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await svc.GetByIdAsync(id, ct) is { } t ? Ok(t) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest req, CancellationToken ct)
    {
        var id = await svc.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest req, CancellationToken ct)
        => await svc.UpdateAsync(id, req, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await svc.DeleteAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService svc) : ControllerBase
{
    // Complex query: task progress % grouped by department (JOIN + aggregate)
    [HttpGet("department-progress")]
    public async Task<IActionResult> DepartmentProgress(CancellationToken ct)
        => Ok(await svc.GetDepartmentProgressAsync(ct));

    // Complex query: per-project rollup with nearest open due date
    [HttpGet("project-summary")]
    public async Task<IActionResult> ProjectSummary(CancellationToken ct)
        => Ok(await svc.GetProjectSummaryAsync(ct));

    // Data export: project summary as CSV
    [HttpGet("project-summary.csv")]
    public async Task<IActionResult> ProjectSummaryCsv(CancellationToken ct)
    {
        var rows = await svc.GetProjectSummaryAsync(ct);
        var sb = new StringBuilder("project_id,project_name,status,owner,department,total_tasks,done_tasks,progress_percent,nearest_due_date\n");
        foreach (var r in rows)
            sb.AppendLine($"{r.ProjectId},\"{r.ProjectName}\",{r.Status},\"{r.OwnerName}\",{r.Department},{r.TotalTasks},{r.DoneTasks},{r.ProgressPercent},{r.NearestDueDate}");
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "project-summary.csv");
    }

    // Data export: department progress as CSV
    [HttpGet("department-progress.csv")]
    public async Task<IActionResult> DepartmentProgressCsv(CancellationToken ct)
    {
        var rows = await svc.GetDepartmentProgressAsync(ct);
        var sb = new StringBuilder("department,total_tasks,done,in_progress,blocked,progress_percent\n");
        foreach (var r in rows)
            sb.AppendLine($"{r.Department},{r.TotalTasks},{r.DoneTasks},{r.InProgressTasks},{r.BlockedTasks},{r.ProgressPercent}");
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "department-progress.csv");
    }
}
