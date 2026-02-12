using System.ComponentModel.DataAnnotations.Schema;

namespace Idopontfoglalo.Core.Entities;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeServiceService> EmployeeServices { get; set; } = new List<EmployeeServiceService>();
}
