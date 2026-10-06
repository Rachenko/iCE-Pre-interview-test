using Microsoft.Extensions.DependencyInjection;

namespace ProjectMonitoring.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
