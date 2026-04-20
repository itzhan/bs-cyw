using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStreetlight.Api.Models.Entities;

[Table("work_order")]
public class WorkOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("order_no"), Required, MaxLength(50)]
    public string OrderNo { get; set; } = "";

    [Column("alarm_id")]
    public long? AlarmId { get; set; }

    [Column("repair_report_id")]
    public long? RepairReportId { get; set; }

    [Column("streetlight_id")]
    public long? StreetlightId { get; set; }

    [Column("area_id")]
    public long? AreaId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public int Priority { get; set; } = 2;

    public int Status { get; set; } = 0;

    [Column("assignee_id")]
    public long? AssigneeId { get; set; }

    [Column("reporter_id")]
    public long? ReporterId { get; set; }

    [Column("expected_finish")]
    public DateTime? ExpectedFinish { get; set; }

    [Column("actual_finish")]
    public DateTime? ActualFinish { get; set; }

    [Column("repair_content")]
    public string? RepairContent { get; set; }

    [Column("repair_cost", TypeName = "decimal(10,2)")]
    public decimal? RepairCost { get; set; }

    [MaxLength(2000)]
    public string? Images { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("StreetlightId")]
    public Streetlight? Streetlight { get; set; }

    [ForeignKey("AreaId")]
    public Area? Area { get; set; }
}
