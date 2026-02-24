using System.ComponentModel.DataAnnotations.Schema;
namespace Idopontfoglalo.Core.Entities;
public class Employee
{
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("location_id")]
    public int? LocationId { get; set; }
    public Location? Location { get; set; }


    public ICollection<EmployeeServiceService> EmployeeServices { get; set; } = new List<EmployeeServiceService>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
