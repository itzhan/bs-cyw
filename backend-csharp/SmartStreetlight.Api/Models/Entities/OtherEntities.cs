using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("repair_report")]
public class RepairReport
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("report_no"), Required, MaxLength(50)]
    public string ReportNo { get; set; } = "";

    [Column("reporter_id")]
    public long? ReporterId { get; set; }

    [Column("reporter_name"), MaxLength(50)]
    public string? ReporterName { get; set; }

    [Column("reporter_phone"), MaxLength(20)]
    public string? ReporterPhone { get; set; }

    [Column("streetlight_id")]
    public long? StreetlightId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? Images { get; set; }

    public int Status { get; set; } = 0;

    [Column("work_order_id")]
    public long? WorkOrderId { get; set; }

    [MaxLength(500)]
    public string? Reply { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

[Table("energy_record")]
public class EnergyRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("streetlight_id")]
    public long? StreetlightId { get; set; }

    [Column("cabinet_id")]
    public long? CabinetId { get; set; }

    [Column("area_id")]
    public long? AreaId { get; set; }

    [Column("record_date")]
    public DateOnly RecordDate { get; set; }

    [Column("energy_kwh", TypeName = "decimal(10,3)")]
    public decimal EnergyKwh { get; set; } = 0;

    [Column("running_minutes")]
    public int RunningMinutes { get; set; } = 0;

    [Column("avg_power", TypeName = "decimal(8,2)")]
    public decimal? AvgPower { get; set; }

    [Column("peak_power", TypeName = "decimal(8,2)")]
    public decimal? PeakPower { get; set; }

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("announcement")]
public class Announcement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Content { get; set; }

    public int Type { get; set; } = 1;

    public int Status { get; set; } = 1;

    [Column("top_flag")]
    public int TopFlag { get; set; } = 0;

    [Column("publisher_id")]
    public long? PublisherId { get; set; }

    [Column("publish_time")]
    public DateTime? PublishTime { get; set; }

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

[Table("mqtt_message")]
public class MqttMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("device_uid")]
    public string? DeviceUid { get; set; }

    public string? Topic { get; set; }

    public string? Payload { get; set; }

    public int Direction { get; set; } = 1;

    public int Status { get; set; } = 1;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
