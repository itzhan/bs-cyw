using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.DTOs;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/streetlights")]
public class StreetlightController : ControllerBase
{
    private readonly AppDbContext _db;
    public StreetlightController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, long? areaId = null,
        long? cabinetId = null, int? onlineStatus = null, int? deviceStatus = null, string? keyword = null)
    {
        var query = _db.Streetlights
            .AsNoTracking()
            .Include(s => s.Area)
            .Include(s => s.Cabinet)
            .AsQueryable();
        if (areaId.HasValue) query = query.Where(s => s.AreaId == areaId);
        if (cabinetId.HasValue) query = query.Where(s => s.CabinetId == cabinetId);
        if (onlineStatus.HasValue) query = query.Where(s => s.OnlineStatus == onlineStatus);
        if (deviceStatus.HasValue) query = query.Where(s => s.DeviceStatus == deviceStatus);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(s => s.Name.Contains(keyword) || s.Code.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query
            .OrderBy(s => s.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new StreetlightListDTO
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                DeviceUid = s.DeviceUid,
                AreaId = s.AreaId,
                AreaName = s.Area != null ? s.Area.Name : null,
                CabinetId = s.CabinetId,
                CabinetName = s.Cabinet != null ? s.Cabinet.Name : null,
                LampType = s.LampType,
                Power = s.Power,
                OnlineStatus = s.OnlineStatus,
                LightStatus = s.LightStatus,
                DeviceStatus = s.DeviceStatus,
                Brightness = s.Brightness,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
        return Ok(Result<object>.Success(new PageResult<StreetlightListDTO>(records, total, page, size)));
    }

    [HttpGet("all")]
    public async Task<IActionResult> All(long? areaId = null)
    {
        var query = _db.Streetlights.AsNoTracking().AsQueryable();
        if (areaId.HasValue) query = query.Where(s => s.AreaId == areaId);
        var list = await query
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Code,
                s.Name,
                s.DeviceUid,
                s.AreaId,
                s.CabinetId,
                s.Address,
                s.Longitude,
                s.Latitude,
                s.LampType,
                s.Power,
                s.Brightness,
                s.OnlineStatus,
                s.LightStatus,
                s.DeviceStatus,
                s.Voltage,
                s.CurrentVal,
                s.Temperature,
                s.RunningHours,
                s.UpdatedAt
            })
            .ToListAsync();
        return Ok(Result<object>.Success(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.Streetlights
            .AsNoTracking()
            .Include(s => s.Area)
            .Include(s => s.Cabinet)
            .Where(s => s.Id == id)
            .Select(s => new StreetlightDetailDTO
            {
                Id = s.Id,
                Code = s.Code,
                DeviceUid = s.DeviceUid,
                Name = s.Name,
                AreaId = s.AreaId,
                AreaName = s.Area != null ? s.Area.Name : null,
                CabinetId = s.CabinetId,
                CabinetName = s.Cabinet != null ? s.Cabinet.Name : null,
                Address = s.Address,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                LampType = s.LampType,
                HardwareModel = s.HardwareModel,
                ElectricalParams = s.ElectricalParams,
                ProtectionRating = s.ProtectionRating,
                Power = s.Power,
                Height = s.Height,
                Brightness = s.Brightness,
                OnlineStatus = s.OnlineStatus,
                LightStatus = s.LightStatus,
                DeviceStatus = s.DeviceStatus,
                InstallDate = s.InstallDate,
                Voltage = s.Voltage,
                CurrentVal = s.CurrentVal,
                Temperature = s.Temperature,
                RunningHours = s.RunningHours,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();
        if (entity == null) return Ok(Result.Error(404, "路灯不存在"));
        return Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] Streetlight streetlight)
    {
        _db.Streetlights.Add(streetlight);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Update(long id, [FromBody] Streetlight dto)
    {
        var entity = await _db.Streetlights.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "路灯不存在"));

        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.DeviceUid != null) entity.DeviceUid = dto.DeviceUid;
        if (dto.Address != null) entity.Address = dto.Address;
        if (dto.Longitude.HasValue) entity.Longitude = dto.Longitude;
        if (dto.Latitude.HasValue) entity.Latitude = dto.Latitude;
        if (dto.LampType != null) entity.LampType = dto.LampType;
        if (dto.HardwareModel != null) entity.HardwareModel = dto.HardwareModel;
        if (dto.ElectricalParams != null) entity.ElectricalParams = dto.ElectricalParams;
        if (dto.ProtectionRating != null) entity.ProtectionRating = dto.ProtectionRating;
        if (dto.Power.HasValue) entity.Power = dto.Power;
        if (dto.Height.HasValue) entity.Height = dto.Height;
        entity.AreaId = dto.AreaId != 0 ? dto.AreaId : entity.AreaId;
        if (dto.CabinetId.HasValue) entity.CabinetId = dto.CabinetId;
        if (dto.DeviceStatus != entity.DeviceStatus) entity.DeviceStatus = dto.DeviceStatus;

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    public class LocationDto { public decimal? Longitude { get; set; } public decimal? Latitude { get; set; } public string? Address { get; set; } }

    [HttpPut("{id}/location")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> UpdateLocation(long id, [FromBody] LocationDto dto)
    {
        var entity = await _db.Streetlights.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "路灯不存在"));
        if (dto.Longitude.HasValue) entity.Longitude = dto.Longitude;
        if (dto.Latitude.HasValue) entity.Latitude = dto.Latitude;
        if (dto.Address != null) entity.Address = dto.Address;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Streetlights.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "路灯不存在"));
        _db.Streetlights.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}
