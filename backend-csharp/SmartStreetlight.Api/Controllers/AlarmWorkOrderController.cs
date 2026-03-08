using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.DTOs;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/alarms")]
public class AlarmController : ControllerBase
{
    private readonly AppDbContext _db;
    public AlarmController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, int? type = null,
        int? level = null, int? status = null, string? keyword = null)
    {
        var query = _db.Alarms.AsQueryable();
        if (type.HasValue) query = query.Where(a => a.Type == type);
        if (level.HasValue) query = query.Where(a => a.Level == level);
        if (status.HasValue) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(a => a.Title.Contains(keyword) || a.AlarmCode.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(a => a.AlarmTime)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<Alarm>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.Alarms.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "告警不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPut("{id}/handle")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Handle(long id, [FromBody] AlarmHandleDTO dto)
    {
        var entity = await _db.Alarms.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "告警不存在"));

        entity.Status = dto.Status;
        entity.HandleRemark = dto.HandleRemark;
        entity.HandleTime = DateTime.Now;

        // Get current user id
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user != null) entity.HandlerId = user.Id;

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}

[ApiController]
[Route("api/work-orders")]
public class WorkOrderController : ControllerBase
{
    private readonly AppDbContext _db;
    public WorkOrderController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, int? status = null,
        long? areaId = null, string? keyword = null)
    {
        var query = _db.WorkOrders.AsQueryable();
        if (status.HasValue) query = query.Where(w => w.Status == status);
        if (areaId.HasValue) query = query.Where(w => w.AreaId == areaId);
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(w => w.Title.Contains(keyword) || w.OrderNo.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<WorkOrder>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.WorkOrders.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "工单不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Create([FromBody] WorkOrderDTO dto)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        var order = new WorkOrder
        {
            OrderNo = $"WO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
            AlarmId = dto.AlarmId,
            StreetlightId = dto.StreetlightId,
            AreaId = dto.AreaId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            AssigneeId = dto.AssigneeId,
            ReporterId = user?.Id,
            ExpectedFinish = dto.ExpectedFinish
        };
        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkOrderDTO dto)
    {
        var entity = await _db.WorkOrders.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "工单不存在"));

        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Description != null) entity.Description = dto.Description;
        entity.Priority = dto.Priority;
        if (dto.AssigneeId.HasValue) entity.AssigneeId = dto.AssigneeId;
        if (dto.ExpectedFinish.HasValue) entity.ExpectedFinish = dto.ExpectedFinish;
        if (dto.RepairContent != null) entity.RepairContent = dto.RepairContent;
        if (dto.RepairCost.HasValue) entity.RepairCost = dto.RepairCost;

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] Dictionary<string, object> body)
    {
        var entity = await _db.WorkOrders.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "工单不存在"));

        if (body.TryGetValue("status", out var statusObj))
            entity.Status = Convert.ToInt32(statusObj);

        if (entity.Status == 3) // Completed
            entity.ActualFinish = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}
