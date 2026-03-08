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
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return Ok(Result.Error(400, "用户名和密码不能为空"));

        var user = await _db.Users.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            return Ok(Result.Error(401, "用户名或密码错误"));

        if (user.Status != 1)
            return Ok(Result.Error(403, "用户已被禁用"));

        user.LastLoginTime = DateTime.Now;
        await _db.SaveChangesAsync();

        var roles = user.Roles.Select(r => r.Name).ToList();
        var token = _jwt.GenerateToken(user.Username, roles);

        return Ok(Result<object>.Success(new
        {
            token,
            user = new
            {
                user.Id,
                user.Username,
                user.RealName,
                user.Phone,
                user.Email,
                user.Avatar,
                user.Status,
                roles
            }
        }));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return Ok(Result.Error(400, "用户名和密码不能为空"));

        if (dto.Username.Length < 3 || dto.Username.Length > 50)
            return Ok(Result.Error(400, "用户名长度3-50位"));
        if (dto.Password.Length < 6 || dto.Password.Length > 30)
            return Ok(Result.Error(400, "密码长度6-30位"));

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return Ok(Result.Error(400, "用户名已存在"));

        var user = new User
        {
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RealName = dto.RealName,
            Phone = dto.Phone,
            Email = dto.Email
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Assign default USER role
        var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "USER");
        if (userRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
            await _db.SaveChangesAsync();
        }

        return Ok(Result.Success("注册成功"));
    }

    [HttpGet("info")]
    [Authorize]
    public async Task<IActionResult> GetInfo()
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));

        return Ok(Result<object>.Success(new
        {
            user.Id,
            user.Username,
            user.RealName,
            user.Phone,
            user.Email,
            user.Avatar,
            user.Status,
            roles = user.Roles.Select(r => r.Name).ToList()
        }));
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO dto)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
            return Ok(Result.Error(400, "旧密码错误"));

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        return Ok(Result.Success("密码修改成功"));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDTO dto)
    {
        var username = User.Identity?.Name;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return Ok(Result.Error(404, "用户不存在"));

        if (dto.RealName != null) user.RealName = dto.RealName;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Avatar != null) user.Avatar = dto.Avatar;
        await _db.SaveChangesAsync();
        return Ok(Result.Success("个人信息更新成功"));
    }
}
