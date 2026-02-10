using System;
using System.ComponentModel.DataAnnotations.Schema;
using Idopontfoglalo.Core.Entities;
namespace Idopontfoglalo.Core.Entities;
public class User
{
    public int Id { get; set; }

    public string Email { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

   
    public Role Role { get; set; }
}
