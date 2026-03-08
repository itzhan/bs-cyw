using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("streetlight")]
public class Streetlight
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = "";

    [Column("device_uid"), MaxLength(100)]
    public string? DeviceUid { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Column("area_id")]
    public long AreaId { get; set; }

    [Column("cabinet_id")]
    public long? CabinetId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    [Column("lamp_type"), MaxLength(50)]
    public string? LampType { get; set; }

    [Column("hardware_model"), MaxLength(100)]
    public string? HardwareModel { get; set; }

    [Column("electrical_params"), MaxLength(500)]
    public string? ElectricalParams { get; set; }

    [Column("protection_rating"), MaxLength(20)]
    public string? ProtectionRating { get; set; }

    public int? Power { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Height { get; set; }

    public int Brightness { get; set; } = 100;

    [Column("online_status")]
    public int OnlineStatus { get; set; } = 1;

    [Column("light_status")]
    public int LightStatus { get; set; } = 0;

    [Column("device_status")]
    public int DeviceStatus { get; set; } = 1;

    [Column("install_date")]
    public DateOnly? InstallDate { get; set; }

    [Column("last_maintain_date")]
    public DateOnly? LastMaintainDate { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal? Voltage { get; set; }

    [Column("current_val", TypeName = "decimal(6,3)")]
    public decimal? CurrentVal { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Temperature { get; set; }

    [Column("running_hours")]
    public int RunningHours { get; set; } = 0;

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey("AreaId")]
    public Area? Area { get; set; }

    [ForeignKey("CabinetId")]
    public Cabinet? Cabinet { get; set; }
}
