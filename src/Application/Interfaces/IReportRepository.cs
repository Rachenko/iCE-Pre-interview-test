namespace ProjectMonitoring.Application;

public interface IReportRepository
{
    Task<IReadOnlyList<DepartmentProgressReport>> GetDepartmentProgressAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSummaryReport>> GetProjectSummaryAsync(CancellationToken ct = default);
}
