using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/areas")]
public class AreaController : ControllerBase
{
    private readonly AppDbContext _db;
    public AreaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(bool includeAll = false, bool withCounts = false)
    {
        var query = _db.Areas.AsQueryable();
        if (!includeAll) query = query.Where(a => a.Status == 1);
        var areas = await query.OrderBy(a => a.SortOrder).ThenBy(a => a.Id).ToListAsync();
        if (!withCounts)
            return Ok(Result<object>.Success(areas));

        var lightCounts = await _db.Streetlights.GroupBy(s => s.AreaId).Select(g => new { AreaId = g.Key, Cnt = g.Count() }).ToDictionaryAsync(x => x.AreaId, x => x.Cnt);
        var cabinetCounts = await _db.Cabinets.GroupBy(c => c.AreaId).Select(g => new { AreaId = g.Key, Cnt = g.Count() }).ToDictionaryAsync(x => x.AreaId, x => x.Cnt);
        var result = areas.Select(a => new {
            a.Id, a.Name, a.Code, a.ParentId, a.Level, a.Description, a.SortOrder, a.Status,
            LightCount = lightCounts.TryGetValue(a.Id, out var lc) ? lc : 0,
            CabinetCount = cabinetCounts.TryGetValue(a.Id, out var cc) ? cc : 0
        }).ToList();
        return Ok(Result<object>.Success(result));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> Tree()
    {
        var all = await _db.Areas.Where(a => a.Status == 1).OrderBy(a => a.SortOrder).ToListAsync();
        var tree = BuildTree(all, 0);
        return Ok(Result<object>.Success(tree));
    }

    [HttpGet("children/{parentId}")]
    public async Task<IActionResult> Children(long parentId)
        => Ok(Result<object>.Success(await _db.Areas.Where(a => a.ParentId == parentId).OrderBy(a => a.SortOrder).ToListAsync()));

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] Area area)
    {
        if (await _db.Areas.AnyAsync(a => a.Code == area.Code))
            return Ok(Result.Error(400, "区域编码已存在"));
        _db.Areas.Add(area);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(long id, [FromBody] Area dto)
    {
        var entity = await _db.Areas.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "区域不存在"));
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Description != null) entity.Description = dto.Description;
        entity.SortOrder = dto.SortOrder;
        entity.ParentId = dto.ParentId;
        if (dto.Level != 0) entity.Level = dto.Level;
        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        if (await _db.Areas.AnyAsync(a => a.ParentId == id))
            return Ok(Result.Error(400, "该区域下存在子区域，无法删除"));
        if (await _db.Streetlights.AnyAsync(s => s.AreaId == id))
            return Ok(Result.Error(400, "该区域下存在路灯，无法删除"));
        if (await _db.Cabinets.AnyAsync(c => c.AreaId == id))
            return Ok(Result.Error(400, "该区域下存在电柜，无法删除"));
        var entity = await _db.Areas.FindAsync(id);
        if (entity != null) _db.Areas.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    private static List<object> BuildTree(List<Area> all, long parentId)
    {
        return all.Where(a => a.ParentId == parentId).Select(a => new
        {
            a.Id, a.Name, a.Code, a.ParentId, a.Level, a.Description, a.SortOrder, a.Status,
            Children = BuildTree(all, a.Id)
        }).Cast<object>().ToList();
    }
}

[ApiController]
[Route("api/cabinets")]
public class CabinetController : ControllerBase
{
    private readonly AppDbContext _db;
    public CabinetController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, long? areaId = null,
        int? status = null, string? keyword = null)
    {
        var query = _db.Cabinets.AsQueryable();
        if (areaId.HasValue) query = query.Where(c => c.AreaId == areaId);
        if (status.HasValue) query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(c => c.Name.Contains(keyword) || c.Code.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderBy(c => c.Id).Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<Cabinet>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.Cabinets.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "电柜不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] Cabinet cabinet)
    {
        _db.Cabinets.Add(cabinet);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Update(long id, [FromBody] Cabinet dto)
    {
        var entity = await _db.Cabinets.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "电柜不存在"));
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Address != null) entity.Address = dto.Address;
        if (dto.Longitude.HasValue) entity.Longitude = dto.Longitude;
        if (dto.Latitude.HasValue) entity.Latitude = dto.Latitude;
        if (dto.Capacity.HasValue) entity.Capacity = dto.Capacity;
        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Cabinets.FindAsync(id);
        if (entity != null) _db.Cabinets.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoleController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(Result<object>.Success(await _db.Roles.ToListAsync()));
}
