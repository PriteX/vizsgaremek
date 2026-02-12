using System.ComponentModel.DataAnnotations.Schema;
namespace Idopontfoglalo.Core.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("start_at")]
    public DateTime StartAt { get; set; }

    [Column("end_at")]
    public DateTime EndAt { get; set; }

   

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    [Column("status")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;


}
