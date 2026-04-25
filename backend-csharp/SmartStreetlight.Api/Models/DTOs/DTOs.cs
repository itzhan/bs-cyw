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

public class UserCreateDTO
{
    public string Username { get; set; } = "";
    public string? Password { get; set; }
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Status { get; set; } = 1;
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

public class AlarmCreateDTO
{
    public int Type { get; set; } = 8;
    public int Level { get; set; } = 2;
    public long? StreetlightId { get; set; }
    public long? CabinetId { get; set; }
    public long? AreaId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public class RepairHandleDTO
{
    public int Status { get; set; }
    public string? Reply { get; set; }
}

public class WorkOrderStatusDTO
{
    public int Status { get; set; }
}

public class AlarmBatchHandleDTO
{
    public long[] Ids { get; set; } = Array.Empty<long>();
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

public class StreetlightListDTO
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? DeviceUid { get; set; }
    public long AreaId { get; set; }
    public string? AreaName { get; set; }
    public long? CabinetId { get; set; }
    public string? CabinetName { get; set; }
    public string? LampType { get; set; }
    public int? Power { get; set; }
    public int OnlineStatus { get; set; }
    public int LightStatus { get; set; }
    public int DeviceStatus { get; set; }
    public int Brightness { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StreetlightDetailDTO
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string? DeviceUid { get; set; }
    public string Name { get; set; } = "";
    public long AreaId { get; set; }
    public string? AreaName { get; set; }
    public long? CabinetId { get; set; }
    public string? CabinetName { get; set; }
    public string? Address { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
    public string? LampType { get; set; }
    public string? HardwareModel { get; set; }
    public string? ElectricalParams { get; set; }
    public string? ProtectionRating { get; set; }
    public int? Power { get; set; }
    public decimal? Height { get; set; }
    public int Brightness { get; set; }
    public int OnlineStatus { get; set; }
    public int LightStatus { get; set; }
    public int DeviceStatus { get; set; }
    public DateOnly? InstallDate { get; set; }
    public decimal? Voltage { get; set; }
    public decimal? CurrentVal { get; set; }
    public decimal? Temperature { get; set; }
    public int RunningHours { get; set; }
    public DateTime UpdatedAt { get; set; }
}
