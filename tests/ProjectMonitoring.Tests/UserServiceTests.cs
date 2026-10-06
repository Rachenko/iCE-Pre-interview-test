using ProjectMonitoring.Application;
using ProjectMonitoring.Domain;
using Xunit;

namespace ProjectMonitoring.Tests;

// Simple in-memory fake so service tests don't touch the database.
file sealed class FakeUserRepository : IUserRepository
{
    public readonly List<User> Store = [];
    public int NextId = 1;

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<User>>(Store.ToList());
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Store.FirstOrDefault(u => u.Id == id));
    public Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        Store.Add(new User { Id = NextId, Username = req.Username, Email = req.Email, FullName = req.FullName, Department = req.Department });
        return Task.FromResult(NextId++);
    }
    public Task<bool> UpdateAsync(int id, UpdateUserRequest req, CancellationToken ct = default)
    {
        var u = Store.FirstOrDefault(x => x.Id == id);
        if (u is null) return Task.FromResult(false);
        u.Email = req.Email; u.FullName = req.FullName; u.Department = req.Department;
        return Task.FromResult(true);
    }
    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Store.RemoveAll(u => u.Id == id) > 0);
}

public sealed class UserServiceTests
{
    [Fact]
    public async Task Create_then_GetById_passes_through_repository()
    {
        var svc = new UserService(new FakeUserRepository());

        var id = await svc.CreateAsync(new CreateUserRequest("dao", "d@x.com", "Dao M", "Data"));
        var user = await svc.GetByIdAsync(id);

        Assert.NotNull(user);
        Assert.Equal("dao", user!.Username);
    }

    [Fact]
    public async Task Delete_missing_returns_false()
    {
        var svc = new UserService(new FakeUserRepository());
        Assert.False(await svc.DeleteAsync(123));
    }
}
