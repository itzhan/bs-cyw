using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.DTOs;
using SmartStreetlight.Api.Models.Entities;
using SmartStreetlight.Api.Services;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/control")]
public class ControlController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MqttPublishService _mqtt;
    private readonly ILogger<ControlController> _logger;
    public ControlController(AppDbContext db, MqttPublishService mqtt, ILogger<ControlController> logger)
    {
        _db = db;
        _mqtt = mqtt;
        _logger = logger;
    }

    [HttpPost("execute")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Execute([FromBody] ControlDTO dto)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        long? operatorId = user?.Id;

        List<Streetlight> lights;
        if (dto.StreetlightIds != null && dto.StreetlightIds.Any())
            lights = await _db.Streetlights.Where(s => dto.StreetlightIds.Contains(s.Id)).ToListAsync();
        else if (dto.AreaId.HasValue)
            lights = await _db.Streetlights.Where(s => s.AreaId == dto.AreaId).ToListAsync();
        else
            return Ok(Result.Error(400, "请指定路灯或区域"));

        int successCount = 0;
        var mqttTasks = new List<Task>();
        foreach (var light in lights)
        {
            if (light.OnlineStatus == 0) continue;
            bool success = true;
            int? targetBrightness = null;
            string mqttPayload = "";
            switch (dto.Action)
            {
                case "TURN_ON":
                    light.LightStatus = 1;
                    light.Brightness = dto.Brightness ?? 100;
                    targetBrightness = light.Brightness;
                    mqttPayload = $"{{\"action\":\"TURN_ON\",\"brightness\":{light.Brightness}}}";
                    break;
                case "TURN_OFF":
                    light.LightStatus = 0;
                    light.Brightness = 0;
                    targetBrightness = 0;
                    mqttPayload = "{\"action\":\"TURN_OFF\"}";
                    break;
                case "SET_BRIGHTNESS":
                    if (dto.Brightness.HasValue)
                    {
                        light.Brightness = dto.Brightness.Value;
                        light.LightStatus = dto.Brightness > 0 ? 1 : 0;
                        targetBrightness = dto.Brightness.Value;
                        mqttPayload = $"{{\"action\":\"SET_BRIGHTNESS\",\"brightness\":{dto.Brightness.Value}}}";
                    }
                    break;
                default:
                    success = false;
                    break;
            }
            if (success) successCount++;

            // 关键修复: 同时下发 MQTT 给真实设备模拟器,
            // 否则模拟器仍持有旧状态, 5s 后上报 status 会把 DB 又覆盖回去。
            if (success && !string.IsNullOrWhiteSpace(light.DeviceUid) && !string.IsNullOrEmpty(mqttPayload))
            {
                try { mqttTasks.Add(_mqtt.SendCommandAsync(light.DeviceUid!, dto.Action, mqttPayload)); }
                catch { /* ignore mqtt errors, DB 已经写了 */ }
            }

            _db.ControlLogs.Add(new ControlLog
            {
                StreetlightId = light.Id,
                AreaId = light.AreaId,
                Action = dto.Action,
                Detail = $"{dto.Action} 亮度{(dto.Brightness.HasValue ? dto.Brightness + "%" : "")}",
                OperatorId = operatorId,
                Result = success ? 1 : 0,
                Remark = dto.Remark
            });
        }
        await _db.SaveChangesAsync();
        if (mqttTasks.Count > 0)
        {
            try { await Task.WhenAll(mqttTasks); }
            catch (Exception ex) { _logger.LogWarning("[Control] MQTT sync failed: {Msg}", ex.Message); }
        }
        return Ok(Result.Success($"成功控制 {successCount} 盏路灯", null));
    }

    [HttpGet("logs")]
    [Authorize(Roles = "ADMIN,OPERATOR")]
    public async Task<IActionResult> Logs(int page = 1, int size = 20)
    {
        var logs = await _db.ControlLogs.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(logs));
    }
}

[ApiController]
[Route("api/strategies")]
public class ControlStrategyController : ControllerBase
{
    private readonly AppDbContext _db;
    public ControlStrategyController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, int? type = null,
        int? status = null, string? keyword = null)
    {
        var query = _db.ControlStrategies.AsQueryable();
        if (type.HasValue) query = query.Where(c => c.Type == type);
        if (status.HasValue) query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(c => c.Name.Contains(keyword));

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(c => c.Priority)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<ControlStrategy>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var entity = await _db.ControlStrategies.FindAsync(id);
        return entity == null ? Ok(Result.Error(404, "策略不存在")) : Ok(Result<object>.Success(entity));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] ControlStrategy strategy)
    {
        _db.ControlStrategies.Add(strategy);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(long id, [FromBody] ControlStrategy dto)
    {
        var entity = await _db.ControlStrategies.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "策略不存在"));

        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.GroupNo.HasValue) entity.GroupNo = dto.GroupNo;
        if (dto.Type != 0) entity.Type = dto.Type;
        if (dto.ActionType != 0) entity.ActionType = dto.ActionType;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.StartTime.HasValue) entity.StartTime = dto.StartTime;
        if (dto.EndTime.HasValue) entity.EndTime = dto.EndTime;
        if (dto.Brightness.HasValue) entity.Brightness = dto.Brightness;
        if (dto.LightThreshold.HasValue) entity.LightThreshold = dto.LightThreshold;
        if (dto.TargetLongitude.HasValue) entity.TargetLongitude = dto.TargetLongitude;
        if (dto.TargetLatitude.HasValue) entity.TargetLatitude = dto.TargetLatitude;
        if (dto.EffectiveStart.HasValue) entity.EffectiveStart = dto.EffectiveStart;
        if (dto.EffectiveEnd.HasValue) entity.EffectiveEnd = dto.EffectiveEnd;
        entity.StartDatetime = dto.StartDatetime;
        entity.EndDatetime = dto.EndDatetime;
        entity.LastPhase = 0; // 编辑后重置调度状态,允许重新触发
        entity.Status = dto.Status;
        if (dto.Priority != 0) entity.Priority = dto.Priority;
        if (dto.AreaId.HasValue) entity.AreaId = dto.AreaId;

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/toggle")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Toggle(long id)
    {
        var entity = await _db.ControlStrategies.FindAsync(id);
        if (entity == null) return Ok(Result.Error(404, "策略不存在"));
        entity.Status = entity.Status == 1 ? 0 : 1;
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.ControlStrategies.FindAsync(id);
        if (entity != null) _db.ControlStrategies.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }
}
