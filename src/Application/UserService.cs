using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Application;

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
