using Microsoft.EntityFrameworkCore;
using ProjectMonitoring.Domain;

namespace ProjectMonitoring.Infrastructure;

public sealed class MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(50);
            e.Property(u => u.Email).HasMaxLength(255);
            e.Property(u => u.FullName).HasMaxLength(150);
            e.Property(u => u.Department).HasMaxLength(100);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("active");
            e.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectTask>(e =>
        {
            e.HasKey(t => t.Id);
            e.ToTable("tasks");
            e.Property(t => t.Title).HasMaxLength(200);
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("todo");
            e.Property(t => t.Priority).HasMaxLength(10).HasDefaultValue("medium");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
            e.HasOne<Project>()
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SystemLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Level).HasMaxLength(10).HasDefaultValue("info");
            e.Property(l => l.Source).HasMaxLength(100);
            e.Property(l => l.Metadata).HasColumnType("jsonb");
            e.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
