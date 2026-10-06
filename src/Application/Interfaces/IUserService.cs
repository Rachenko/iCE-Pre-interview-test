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
