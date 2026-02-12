using System.ComponentModel.DataAnnotations.Schema;
using Idopontfoglalo.Core.Entities;

namespace Idopontfoglalo.Core.Entities;

[Table("EmployeeServices")]
public class EmployeeServiceService
{
    [Column("employee_id")] 
    public int EmployeeId { get; set; }

    public Employee Employee{ get; set; } = null!;

    [Column("service_id")]  
    public int ServiceId { get; set; }

    public Service Service { get; set; } = null!;
}

