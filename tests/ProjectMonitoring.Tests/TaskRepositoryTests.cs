using Microsoft.EntityFrameworkCore;
using ProjectMonitoring.Application;
using ProjectMonitoring.Domain;
using ProjectMonitoring.Infrastructure;
using Xunit;

namespace ProjectMonitoring.Tests;

public sealed class TaskRepositoryTests : IDisposable
{
    private readonly MonitoringDbContext _db;
    private readonly TaskRepository _repo;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MonitoringDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new MonitoringDbContext(options);
        _repo = new TaskRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    private ProjectTask SeedTask(string status = "todo", int projectId = 1)
    {
        var task = new ProjectTask { ProjectId = projectId, Title = "seed", Status = status, Priority = "high" };
        _db.Tasks.Add(task);
        _db.SaveChanges();
        return task;
    }

    [Fact]
    public async Task CreateAndGetById_round_trips()
    {
        var id = await _repo.CreateAsync(new CreateTaskRequest(1, "Write tests", "cover repos", "low", 2, new DateOnly(2026, 12, 1)));
        var task = await _repo.GetByIdAsync(id);

        Assert.NotNull(task);
        Assert.Equal("Write tests", task.Title);
        Assert.Equal("low", task.Priority);
        Assert.Equal(2, task.AssigneeId);
    }

    [Fact]
    public async Task GetAll_filters_by_project_and_status()
    {
        SeedTask("todo", projectId: 1);
        SeedTask("done", projectId: 1);
        SeedTask("todo", projectId: 2);

        var filtered = await _repo.GetAllAsync(projectId: 1, status: "todo");

        Assert.Single(filtered);
        Assert.Equal(1, filtered[0].ProjectId);
        Assert.Equal("todo", filtered[0].Status);
    }

    [Fact]
    public async Task Update_sets_CompletedAt_when_status_becomes_done()
    {
        var task = SeedTask("in_progress");

        var updated = await _repo.UpdateAsync(task.Id,
            new UpdateTaskRequest("t", null, "done", "high", null, null));

        Assert.True(updated);
        var reloaded = await _repo.GetByIdAsync(task.Id);
        Assert.Equal("done", reloaded!.Status);
        Assert.NotNull(reloaded.CompletedAt);
    }

    [Fact]
    public async Task Update_clears_CompletedAt_when_status_leaves_done()
    {
        var task = SeedTask("done");
        task.CompletedAt = DateTime.UtcNow.AddDays(-1);
        _db.SaveChanges();

        await _repo.UpdateAsync(task.Id,
            new UpdateTaskRequest("t", null, "todo", "high", null, null));

        var reloaded = await _repo.GetByIdAsync(task.Id);
        Assert.Equal("todo", reloaded!.Status);
        Assert.Null(reloaded.CompletedAt);
    }

    [Fact]
    public async Task Update_returns_false_for_missing_id()
    {
        var updated = await _repo.UpdateAsync(999,
            new UpdateTaskRequest("t", null, "done", "high", null, null));
        Assert.False(updated);
    }

    [Fact]
    public async Task Delete_removes_task()
    {
        var task = SeedTask();

        Assert.True(await _repo.DeleteAsync(task.Id));
        Assert.Null(await _repo.GetByIdAsync(task.Id));
        Assert.False(await _repo.DeleteAsync(task.Id));
    }
}
