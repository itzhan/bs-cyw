using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Common;
using SmartStreetlight.Api.Data;

namespace SmartStreetlight.Api.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatisticsController(AppDbContext db) => _db = db;

    [HttpGet("overview")]
    public async Task<IActionResult> Overview()
    {
        var total = await _db.Streetlights.CountAsync();
        var online = await _db.Streetlights.CountAsync(s => s.OnlineStatus == 1);
        var lightOn = await _db.Streetlights.CountAsync(s => s.LightStatus == 1);
        var fault = await _db.Streetlights.CountAsync(s => s.DeviceStatus == 0);
        var totalCabinets = await _db.Cabinets.CountAsync();
        var unhandledAlarms = await _db.Alarms.CountAsync(a => a.Status == 0);
        var todayAlarms = await _db.Alarms.CountAsync(a => a.AlarmTime >= DateTime.Today);
        var pendingOrders = await _db.WorkOrders.CountAsync(w => w.Status == 0 || w.Status == 1);
        var pendingReports = await _db.RepairReports.CountAsync(r => r.Status == 0);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var todayEnergy = await _db.EnergyRecords
            .Where(e => e.RecordDate == today).SumAsync(e => (decimal?)e.EnergyKwh) ?? 0;
        var monthEnergy = await _db.EnergyRecords
            .Where(e => e.RecordDate >= monthStart && e.RecordDate <= today).SumAsync(e => (decimal?)e.EnergyKwh) ?? 0;

        return Ok(Result<object>.Success(new
        {
            totalLights = total,
            onlineCount = online,
            onlineRate = total > 0 ? Math.Round(online * 10000.0 / total) / 100.0 : 0,
            lightOnCount = lightOn,
            lightOnRate = total > 0 ? Math.Round(lightOn * 10000.0 / total) / 100.0 : 0,
            faultCount = fault,
            totalCabinets,
            unhandledAlarms,
            todayAlarms,
            pendingOrders,
            pendingReports,
            todayEnergy,
            monthEnergy
        }));
    }

    [HttpGet("energy/daily")]
    public async Task<IActionResult> EnergyDaily(string? startDate = null, string? endDate = null)
    {
        var start = startDate != null ? DateOnly.Parse(startDate) : DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = endDate != null ? DateOnly.Parse(endDate) : DateOnly.FromDateTime(DateTime.Today);

        var data = await _db.EnergyRecords
            .Where(e => e.RecordDate >= start && e.RecordDate <= end)
            .GroupBy(e => e.RecordDate)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), energy = g.Sum(e => e.EnergyKwh) })
            .OrderBy(x => x.date)
            .ToListAsync();
        return Ok(Result<object>.Success(data));
    }

    [HttpGet("energy/by-area")]
    public async Task<IActionResult> EnergyByArea(string? startDate = null, string? endDate = null)
    {
        var start = startDate != null ? DateOnly.Parse(startDate) : DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = endDate != null ? DateOnly.Parse(endDate) : DateOnly.FromDateTime(DateTime.Today);

        var data = await _db.EnergyRecords
            .Where(e => e.RecordDate >= start && e.RecordDate <= end)
            .GroupBy(e => e.AreaId)
            .Select(g => new object[] { g.Key!, g.Sum(e => e.EnergyKwh) })
            .ToListAsync();
        return Ok(Result<object>.Success(data));
    }

    [HttpGet("alarm/by-type")]
    public async Task<IActionResult> AlarmByType()
    {
        var since = DateTime.Now.AddDays(-30);
        var typeNames = new Dictionary<int, string>
        {
            { 1, "灯具故障" }, { 2, "电压异常" }, { 3, "电流异常" }, { 4, "通信故障" },
            { 5, "线缆异常" }, { 6, "温度异常" }, { 7, "漏电告警" }, { 8, "其他" }
        };
        var data = await _db.Alarms.Where(a => a.AlarmTime >= since)
            .GroupBy(a => a.Type)
            .Select(g => new { type = g.Key, count = g.Count() })
            .ToListAsync();
        return Ok(Result<object>.Success(data.Select(d => new
        {
            d.type, typeName = typeNames.GetValueOrDefault(d.type, "其他"), d.count
        })));
    }

    [HttpGet("alarm/by-level")]
    public async Task<IActionResult> AlarmByLevel()
    {
        var since = DateTime.Now.AddDays(-30);
        var levelNames = new Dictionary<int, string> { { 1, "低" }, { 2, "中" }, { 3, "高" }, { 4, "紧急" } };
        var data = await _db.Alarms.Where(a => a.AlarmTime >= since)
            .GroupBy(a => a.Level)
            .Select(g => new { level = g.Key, count = g.Count() })
            .ToListAsync();
        return Ok(Result<object>.Success(data.Select(d => new
        {
            d.level, levelName = levelNames.GetValueOrDefault(d.level, "未知"), d.count
        })));
    }

    [HttpGet("device/by-type")]
    public async Task<IActionResult> DeviceByType()
    {
        var data = await _db.Streetlights.GroupBy(s => s.LampType)
            .Select(g => new { lampType = g.Key, count = g.Count() }).ToListAsync();
        return Ok(Result<object>.Success(data));
    }

    [HttpGet("device/by-area")]
    public async Task<IActionResult> DeviceByArea()
    {
        var data = await _db.Streetlights.GroupBy(s => s.AreaId)
            .Select(g => new { areaId = g.Key, count = g.Count() }).ToListAsync();
        return Ok(Result<object>.Success(data));
    }
}
