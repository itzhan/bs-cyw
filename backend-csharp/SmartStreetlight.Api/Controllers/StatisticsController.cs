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
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var streetlightStats = await _db.Streetlights
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Online = g.Count(s => s.OnlineStatus == 1),
                LightOn = g.Count(s => s.LightStatus == 1),
                Fault = g.Count(s => s.DeviceStatus == 0)
            })
            .FirstOrDefaultAsync();
        var totalCabinets = await _db.Cabinets.AsNoTracking().CountAsync();
        var alarmStats = await _db.Alarms
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Unhandled = g.Count(a => a.Status == 0),
                Today = g.Count(a => a.AlarmTime >= DateTime.Today)
            })
            .FirstOrDefaultAsync();
        var workOrderStats = await _db.WorkOrders
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Pending = g.Count(w => w.Status == 0 || w.Status == 1)
            })
            .FirstOrDefaultAsync();
        var repairStats = await _db.RepairReports
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Pending = g.Count(r => r.Status == 0)
            })
            .FirstOrDefaultAsync();
        var energyStats = await _db.EnergyRecords
            .AsNoTracking()
            .Where(e => e.RecordDate >= monthStart && e.RecordDate <= today)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TodayEnergy = g.Where(e => e.RecordDate == today).Sum(e => (decimal?)e.EnergyKwh) ?? 0,
                MonthEnergy = g.Sum(e => (decimal?)e.EnergyKwh) ?? 0
            })
            .FirstOrDefaultAsync();

        var total = streetlightStats?.Total ?? 0;
        var online = streetlightStats?.Online ?? 0;
        var lightOn = streetlightStats?.LightOn ?? 0;
        var fault = streetlightStats?.Fault ?? 0;
        var unhandledAlarms = alarmStats?.Unhandled ?? 0;
        var todayAlarms = alarmStats?.Today ?? 0;
        var pendingOrders = workOrderStats?.Pending ?? 0;
        var pendingReports = repairStats?.Pending ?? 0;
        var todayEnergy = energyStats?.TodayEnergy ?? 0;
        var monthEnergy = energyStats?.MonthEnergy ?? 0;

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

        var rows = await _db.EnergyRecords
            .AsNoTracking()
            .Where(e => e.RecordDate >= start && e.RecordDate <= end)
            .GroupBy(e => e.RecordDate)
            .Select(g => new { recordDate = g.Key, energy = g.Sum(e => e.EnergyKwh) })
            .OrderBy(x => x.recordDate)
            .ToListAsync();

        var data = rows.Select(x => new
        {
            date = x.recordDate.ToString("yyyy-MM-dd"),
            x.energy
        });
        return Ok(Result<object>.Success(data));
    }

    [HttpGet("energy/by-area")]
    public async Task<IActionResult> EnergyByArea(string? startDate = null, string? endDate = null)
    {
        var start = startDate != null ? DateOnly.Parse(startDate) : DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = endDate != null ? DateOnly.Parse(endDate) : DateOnly.FromDateTime(DateTime.Today);

        var data = await _db.EnergyRecords
            .AsNoTracking()
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
            .AsNoTracking()
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
            .AsNoTracking()
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
        var data = await _db.Streetlights.AsNoTracking().GroupBy(s => s.LampType)
            .Select(g => new { lampType = g.Key, count = g.Count() }).ToListAsync();
        return Ok(Result<object>.Success(data));
    }

    [HttpGet("device/by-area")]
    public async Task<IActionResult> DeviceByArea()
    {
        var data = await _db.Streetlights.AsNoTracking().GroupBy(s => s.AreaId)
            .Select(g => new { areaId = g.Key, count = g.Count() }).ToListAsync();
        return Ok(Result<object>.Success(data));
    }
}
