using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.DTOs;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/repair-reports")]
public class RepairReportController : ControllerBase
{
    private readonly AppDbContext _db;
    public RepairReportController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> List(int page = 1, int size = 10, int? status = null, string? keyword = null)
    {
        var query = _db.RepairReports.AsQueryable();
        if (status.HasValue) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => r.ReporterName!.Contains(keyword) || r.ReportNo.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<RepairReport>(records, total, page, size)));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> My(int page = 1, int size = 10)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));

        var query = _db.RepairReports.Where(r => r.ReporterId == user.Id).OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync();
        var records = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<RepairReport>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.RepairReports.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "报修不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] RepairReportDTO dto)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        var report = new RepairReport
        {
            ReportNo = $"RP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
            ReporterId = user?.Id,
            ReporterName = dto.ReporterName ?? user?.RealName,
            ReporterPhone = dto.ReporterPhone ?? user?.Phone,
            StreetlightId = dto.StreetlightId,
            Address = dto.Address,
            Longitude = dto.Longitude,
            Latitude = dto.Latitude,
            Description = dto.Description,
            Images = dto.Images
        };
        _db.RepairReports.Add(report);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/handle")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Handle(long id, [FromBody] Dictionary<string, object> body)
    {
        var entity = await _db.RepairReports.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "报修不存在"));

        if (body.TryGetValue("status", out var s)) entity.Status = Convert.ToInt32(s);
        if (body.TryGetValue("reply", out var r)) entity.Reply = r?.ToString();
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}

[ApiController]
[Route("api/announcements")]
public class AnnouncementController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnnouncementController(AppDbContext db) => _db = db;

    [HttpGet("published")]
    public async Task<IActionResult> Published()
    {
        var list = await _db.Announcements.Where(a => a.Status == 1)
            .OrderByDescending(a => a.TopFlag).ThenByDescending(a => a.PublishTime).ToListAsync();
        return Ok(Result<object>.Success(list));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> List(int page = 1, int size = 10, int? status = null, int? type = null)
    {
        var query = _db.Announcements.AsQueryable();
        if (status.HasValue) query = query.Where(a => a.Status == status);
        if (type.HasValue) query = query.Where(a => a.Type == type);

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<Announcement>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.Announcements.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "公告不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] Announcement announcement)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        announcement.PublisherId = user?.Id;
        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(long id, [FromBody] Announcement dto)
    {
        var entity = await _db.Announcements.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "公告不存在"));
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Content != null) entity.Content = dto.Content;
        if (dto.Type != 0) entity.Type = dto.Type;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/publish")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Publish(long id)
    {
        var entity = await _db.Announcements.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "公告不存在"));
        entity.Status = 1;
        entity.PublishTime = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/withdraw")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Withdraw(long id)
    {
        var entity = await _db.Announcements.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "公告不存在"));
        entity.Status = 2;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Announcements.FindAsync(id);
        if (entity != null) _db.Announcements.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}
