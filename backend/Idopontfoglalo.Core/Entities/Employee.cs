namespace Idopontfoglalo.Core.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
