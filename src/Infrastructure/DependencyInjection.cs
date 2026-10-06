using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProjectMonitoring.Application;

namespace ProjectMonitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        services.AddSingleton(builder.Build());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        return services;
    }
}
