using ProjectMonitoring.Application;
using ProjectMonitoring.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=monitoring;Username=postgres;Password=postgres";
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connStr);

builder.Services.AddHealthChecks().AddNpgSql(connStr);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
