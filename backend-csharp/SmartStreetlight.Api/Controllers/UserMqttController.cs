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
[Route("api/users")]
[Authorize(Roles = "ADMIN")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    public UserController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int size = 10, string? keyword = null)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.Username.Contains(keyword) || (u.RealName != null && u.RealName.Contains(keyword)));

        var total = await query.CountAsync();
        var records = await query.OrderBy(u => u.Id).Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<User>(records, total, page, size)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(long id)
    {
        var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? Ok(Result.Error(404, "用户不存在")) : Ok(Result<object>.Success(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UserUpdateDTO dto)
    {
        var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));

        if (dto.RealName != null) user.RealName = dto.RealName;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Avatar != null) user.Avatar = dto.Avatar;
        if (dto.Status.HasValue) user.Status = dto.Status.Value;

        if (dto.RoleIds != null)
        {
            // Remove existing roles and add new ones
            var existingRoles = await _db.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            _db.UserRoles.RemoveRange(existingRoles);
            foreach (var roleId in dto.RoleIds)
            {
                _db.UserRoles.Add(new UserRole { UserId = id, RoleId = roleId });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null) _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok(Result.Success());
    }

    [HttpPut("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));
        user.Password = BCrypt.Net.BCrypt.HashPassword("123456");
        await _db.SaveChangesAsync();
        return Ok(Result.Success("密码已重置为123456"));
    }
}

[ApiController]
[Route("api/mqtt")]
[Authorize(Roles = "ADMIN,OPERATOR")]
public class MqttController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MqttPublishService _mqtt;

    public MqttController(AppDbContext db, MqttPublishService mqtt)
    {
        _db = db;
        _mqtt = mqtt;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        return Ok(Result<object>.Success(new
        {
            connected = _mqtt.IsConnected,
            broker = _mqtt.BrokerInfo,
            totalUplink = await _db.MqttMessages.CountAsync(m => m.Direction == 1),
            totalDownlink = await _db.MqttMessages.CountAsync(m => m.Direction == 2)
        }));
    }

    [HttpGet("messages")]
    public async Task<IActionResult> Messages(int page = 1, int size = 20,
        string? deviceUid = null, int? direction = null)
    {
        var query = _db.MqttMessages.AsQueryable();
        if (!string.IsNullOrWhiteSpace(deviceUid)) query = query.Where(m => m.DeviceUid == deviceUid);
        if (direction.HasValue) query = query.Where(m => m.Direction == direction);

        var total = await query.CountAsync();
        var records = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(Result<object>.Success(new PageResult<MqttMessage>(records, total, page, size)));
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] Dictionary<string, string> body)
    {
        if (!body.TryGetValue("deviceUid", out var deviceUid) || !body.TryGetValue("payload", out var payload))
            return Ok(Result.Error(400, "deviceUid 和 payload 不能为空"));

        body.TryGetValue("action", out var action);
        var success = await _mqtt.SendCommandAsync(deviceUid, action ?? "", payload);
        return Ok(success
            ? Result.Success("指令下发成功", null)
            : Result.Success("指令已记录，但 MQTT 未连接", null));
    }
}
