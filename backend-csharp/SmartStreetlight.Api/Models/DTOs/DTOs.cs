namespace SmartStreetlight.Api.Models.DTOs;

public class LoginDTO
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterDTO
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class PasswordDTO
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class UserUpdateDTO
{
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public int? Status { get; set; }
    public long[]? RoleIds { get; set; }
}

public class ControlDTO
{
    public List<long>? StreetlightIds { get; set; }
    public long? AreaId { get; set; }
    public string Action { get; set; } = "";
    public int? Brightness { get; set; }
    public string? Remark { get; set; }
}

public class AlarmHandleDTO
{
    public int Status { get; set; }
    public string? HandleRemark { get; set; }
}

public class WorkOrderDTO
{
    public long? AlarmId { get; set; }
    public long? StreetlightId { get; set; }
    public long? AreaId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Priority { get; set; } = 2;
    public long? AssigneeId { get; set; }
    public DateTime? ExpectedFinish { get; set; }
    public string? RepairContent { get; set; }
    public decimal? RepairCost { get; set; }
}

public class RepairReportDTO
{
    public string? ReporterName { get; set; }
    public string? ReporterPhone { get; set; }
    public long? StreetlightId { get; set; }
    public string? Address { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public string? Description { get; set; }
    public string? Images { get; set; }
}
