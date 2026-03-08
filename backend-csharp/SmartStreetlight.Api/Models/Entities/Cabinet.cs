using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("cabinet")]
public class Cabinet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = "";

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Column("area_id")]
    public long AreaId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    public int? Capacity { get; set; }

    public int Status { get; set; } = 1;

    [Column("install_date")]
    public DateOnly? InstallDate { get; set; }

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey("AreaId")]
    public Area? Area { get; set; }
}
