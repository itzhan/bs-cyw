using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
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
        var query = _db.Streetlights.AsQueryable();
        if (areaId.HasValue) query = query.Where(s => s.AreaId == areaId);
        if (cabinetId.HasValue) query = query.Where(s => s.CabinetId == cabinetId);
        if (onlineStatus.HasValue) query = query.Where(s => s.OnlineStatus == onlineStatus);
        if (deviceStatus.HasValue) query = query.Where(s => s.DeviceStatus == deviceStatus);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(s => s.Name.Contains(keyword) || s.Code.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderBy(s => s.Id).Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<Streetlight>(records, total, page, size)));
    }

    [HttpGet("all")]
    public async Task<IActionResult> All(long? areaId = null)
    {
        var query = _db.Streetlights.AsQueryable();
        if (areaId.HasValue) query = query.Where(s => s.AreaId == areaId);
        return Ok(Result<object>.Success(await query.ToListAsync()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.Streetlights.FindAsync(id);
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
