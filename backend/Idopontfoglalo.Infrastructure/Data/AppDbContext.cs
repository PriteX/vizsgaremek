using Idopontfoglalo.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Idopontfoglalo.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeService> EmployeeServices => Set<EmployeeService>();
    public DbSet<Availability> Availability => Set<Availability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<EmployeeService>()
            .HasKey(es => new { es.EmployeeId, es.ServiceId });

        modelBuilder.Entity<EmployeeService>()
            .HasOne(es => es.Employee)
            .WithMany(e => e.EmployeeServices)
            .HasForeignKey(es => es.EmployeeId);

        modelBuilder.Entity<EmployeeService>()
            .HasOne(es => es.Service)
            .WithMany(s => s.EmployeeServices)
            .HasForeignKey(es => es.ServiceId);

        modelBuilder.Entity<Availability>()
            .Property(a => a.StartTime)
            .HasColumnType("time");

        modelBuilder.Entity<Availability>()
            .Property(a => a.EndTime)
            .HasColumnType("time");

        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<byte>();

        base.OnModelCreating(modelBuilder);
    }
}
