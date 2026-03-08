using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("user")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = "";

    [Required, MaxLength(255)]
    [JsonIgnore]
    public string Password { get; set; } = "";

    [Column("real_name"), MaxLength(50)]
    public string? RealName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Avatar { get; set; }

    public int Status { get; set; } = 1;

    [Column("last_login_time")]
    public DateTime? LastLoginTime { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    [JsonPropertyName("roles")]
    public List<Role> Roles { get; set; } = new();
}

[Table("role")]
public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    [MaxLength(200)]
    public string? Description { get; set; }

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public List<User> Users { get; set; } = new();
}

[Table("user_role")]
public class UserRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }
}
