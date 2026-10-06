using Microsoft.EntityFrameworkCore;
using ProjectMonitoring.Application;
using ProjectMonitoring.Infrastructure;
using Xunit;

namespace ProjectMonitoring.Tests;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly MonitoringDbContext _db;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MonitoringDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new MonitoringDbContext(options);
        _repo = new UserRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Create_then_GetAll_returns_user()
    {
        await _repo.CreateAsync(new CreateUserRequest("anong", "a@x.com", "Anong S", "Engineering"));

        var users = await _repo.GetAllAsync();

        Assert.Single(users);
        Assert.Equal("anong", users[0].Username);
        Assert.Equal("Engineering", users[0].Department);
    }

    [Fact]
    public async Task GetById_returns_null_when_missing()
    {
        Assert.Null(await _repo.GetByIdAsync(42));
    }

    [Fact]
    public async Task Update_changes_fields()
    {
        var id = await _repo.CreateAsync(new CreateUserRequest("u", "e@x.com", "Name", "Ops"));

        Assert.True(await _repo.UpdateAsync(id, new UpdateUserRequest("e2@x.com", "New Name", "Data")));

        var user = await _repo.GetByIdAsync(id);
        Assert.Equal("e2@x.com", user!.Email);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("Data", user.Department);
    }

    [Fact]
    public async Task Delete_returns_true_then_false()
    {
        var id = await _repo.CreateAsync(new CreateUserRequest("u", "e@x.com", "N", "Ops"));

        Assert.True(await _repo.DeleteAsync(id));
        Assert.False(await _repo.DeleteAsync(id));
    }
}
