using Idopontfoglalo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using EmployeeServiceEntity = Idopontfoglalo.Core.Entities.EmployeeServiceService;

namespace Idopontfoglalo.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeServiceService> EmployeeServices => Set<EmployeeServiceService>();
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

        modelBuilder.Entity<Location>()
       .HasIndex(l => l.Name)
       .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Location)
            .WithMany(l => l.Employees)
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Idopontfoglalo.Core.Entities.EmployeeServiceService>()
     .HasKey(es => new { es.EmployeeId, es.ServiceId });

        modelBuilder.Entity<Idopontfoglalo.Core.Entities.EmployeeServiceService>()
            .HasOne(es => es.Employee)
            .WithMany(e => e.EmployeeServices)
            .HasForeignKey(es => es.EmployeeId);

        modelBuilder.Entity<Idopontfoglalo.Core.Entities.EmployeeServiceService>()
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
