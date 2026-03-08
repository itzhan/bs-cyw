using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("area")]
public class Area
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(50)]
    public string Code { get; set; } = "";

    [Column("parent_id")]
    public long ParentId { get; set; } = 0;

    public int Level { get; set; } = 1;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    public int Status { get; set; } = 1;

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
