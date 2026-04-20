using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("control_log")]
public class ControlLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("streetlight_id")]
    public long? StreetlightId { get; set; }

    [Column("area_id")]
    public long? AreaId { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = "";

    [MaxLength(500)]
    public string? Detail { get; set; }

    [Column("operator_id")]
    public long? OperatorId { get; set; }

    [Column("strategy_id")]
    public long? StrategyId { get; set; }

    public int Result { get; set; } = 1;

    [MaxLength(500)]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("control_strategy")]
public class ControlStrategy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Column("group_no")]
    public int? GroupNo { get; set; }

    [Required]
    public int Type { get; set; }

    [Column("action_type")]
    public int ActionType { get; set; } = 1;

    [Column("area_id")]
    public long? AreaId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("start_time")]
    public TimeOnly? StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly? EndTime { get; set; }

    public int? Brightness { get; set; }

    [Column("light_threshold")]
    public int? LightThreshold { get; set; }

    [Column("target_longitude", TypeName = "decimal(10,7)")]
    public decimal? TargetLongitude { get; set; }

    [Column("target_latitude", TypeName = "decimal(10,7)")]
    public decimal? TargetLatitude { get; set; }

    [Column("effective_start")]
    public DateOnly? EffectiveStart { get; set; }

    [Column("effective_end")]
    public DateOnly? EffectiveEnd { get; set; }

    [Column("start_datetime")]
    public DateTime? StartDatetime { get; set; }

    [Column("end_datetime")]
    public DateTime? EndDatetime { get; set; }

    [Column("last_phase")]
    public int LastPhase { get; set; } = 0;

    public int Status { get; set; } = 1;

    public int Priority { get; set; } = 0;

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("AreaId")]
    public Area? Area { get; set; }
}
