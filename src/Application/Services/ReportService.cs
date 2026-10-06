namespace ProjectMonitoring.Application;

public sealed class ReportService(IReportRepository repo) : IReportService
{
    public Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default)
        => repo.GetDepartmentProgressAsync(ct);
    public Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default)
        => repo.GetProjectSummaryAsync(ct);
}
