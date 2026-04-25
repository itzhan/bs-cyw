using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "ADMIN")]
public class SystemSettingController : ControllerBase
{
    private readonly AppDbContext _db;
    public SystemSettingController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var list = await _db.SystemSettings.OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();
        return Ok(Result<object>.Success(list));
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var entity = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (entity == null) return Ok(Result.Error(404, "配置项不存在"));
        return Ok(Result<object>.Success(entity));
    }

    public class UpdateDto { public string? Value { get; set; } public string? Description { get; set; } }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateDto dto)
    {
        var entity = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (entity == null) return Ok(Result.Error(404, "配置项不存在"));
        if (dto.Value != null) entity.Value = dto.Value;
        if (dto.Description != null) entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}
