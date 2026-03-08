using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("alarm")]
public class Alarm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("alarm_code"), Required, MaxLength(50)]
    public string AlarmCode { get; set; } = "";

    [Required]
    public int Type { get; set; }

    public int Level { get; set; } = 2;

    [Column("streetlight_id")]
    public long? StreetlightId { get; set; }

    [Column("cabinet_id")]
    public long? CabinetId { get; set; }

    [Column("area_id")]
    public long? AreaId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public int Status { get; set; } = 0;

    [Column("handler_id")]
    public long? HandlerId { get; set; }

    [Column("handle_time")]
    public DateTime? HandleTime { get; set; }

    [Column("handle_remark"), MaxLength(500)]
    public string? HandleRemark { get; set; }

    [Column("alarm_time")]
    public DateTime AlarmTime { get; set; } = DateTime.Now;

    [Column("created_at")]
    [JsonIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("StreetlightId")]
    public Streetlight? Streetlight { get; set; }

    [ForeignKey("AreaId")]
    public Area? Area { get; set; }
}
