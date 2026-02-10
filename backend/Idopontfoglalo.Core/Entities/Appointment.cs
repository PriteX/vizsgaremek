namespace Idopontfoglalo.Core.Entities;

public class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
