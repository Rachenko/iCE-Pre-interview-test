namespace ProjectMonitoring.Application;

public interface IReportService
{
    Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default);
}

public sealed class ReportService(IReportRepository repo) : IReportService
{
    public Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default)
        => repo.GetDepartmentProgressAsync(ct);
    public Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default)
        => repo.GetProjectSummaryAsync(ct);
}
