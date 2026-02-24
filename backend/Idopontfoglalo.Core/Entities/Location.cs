using System.ComponentModel.DataAnnotations.Schema;

namespace Idopontfoglalo.Core.Entities;

public class Location
{
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}