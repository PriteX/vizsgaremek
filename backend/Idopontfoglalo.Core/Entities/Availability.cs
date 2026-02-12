using System.ComponentModel.DataAnnotations.Schema;

namespace Idopontfoglalo.Core.Entities;

public class Availability
{
    public int Id { get; set; }
    [Column("employee_id")]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Column("day_of_week")]
    public int DayOfWeek { get; set; }
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }
    [Column("valid_from")]
    public DateOnly? ValidFrom { get; set; }
    [Column("valid_to")]
    public DateOnly? ValidTo { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
