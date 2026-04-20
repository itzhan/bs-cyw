using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Services;

/// <summary>
/// 按策略的 start_datetime / end_datetime 到点批量控灯。
/// brightness=0 视为"关灯+离线(灰)";brightness>0 视为"开灯+恢复在线"。
/// last_phase: 0 未开始, 1 执行中, 2 已结束。
/// </summary>
public class StrategySchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StrategySchedulerService> _logger;
    private readonly MqttPublishService _mqtt;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public StrategySchedulerService(IServiceScopeFactory scopeFactory, ILogger<StrategySchedulerService> logger, MqttPublishService mqtt)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mqtt = mqtt;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[StrategyScheduler] started, scanning every 5s");
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(); }
            catch (Exception ex) { _logger.LogWarning("[StrategyScheduler] tick failed: {Msg}", ex.Message); }
            try { await Task.Delay(Interval, stoppingToken); } catch (TaskCanceledException) { break; }
        }
    }

    private async Task TickAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.Now;

        var strategies = await db.ControlStrategies
            .Where(s => s.Status == 1 && (s.StartDatetime != null || s.EndDatetime != null))
            .ToListAsync();

        foreach (var s in strategies)
        {
            // 开始: last_phase=0 且 now >= start_datetime
            if (s.LastPhase == 0 && s.StartDatetime.HasValue && now >= s.StartDatetime.Value)
            {
                await ApplyActionAsync(db, s, isEnd: false);
                s.LastPhase = 1;
            }
            // 结束: last_phase=1 且 now >= end_datetime
            if (s.LastPhase == 1 && s.EndDatetime.HasValue && now >= s.EndDatetime.Value)
            {
                await ApplyActionAsync(db, s, isEnd: true);
                s.LastPhase = 2;
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task ApplyActionAsync(AppDbContext db, ControlStrategy s, bool isEnd)
    {
        var query = db.Streetlights.AsQueryable();
        if (s.AreaId.HasValue) query = query.Where(l => l.AreaId == s.AreaId.Value);
        var lights = await query.ToListAsync();
        if (lights.Count == 0) return;

        // 开始阶段按 brightness 决定动作;结束阶段默认恢复为在线且开灯 100%
        bool turnOff;
        int brightness;
        if (isEnd)
        {
            turnOff = false;
            brightness = 100;
        }
        else
        {
            brightness = s.Brightness ?? 100;
            turnOff = brightness == 0;
        }

        foreach (var l in lights)
        {
            if (turnOff)
            {
                l.LightStatus = 0;
                l.Brightness = 0;
                l.OnlineStatus = 0; // 关灯同时置为离线(地图显示灰色)
            }
            else
            {
                l.LightStatus = 1;
                l.Brightness = brightness;
                l.OnlineStatus = 1;
            }

            // 下发 MQTT 指令,让设备模拟器同步状态,避免其下次上报时覆盖 DB
            if (!string.IsNullOrWhiteSpace(l.DeviceUid))
            {
                try
                {
                    var payload = turnOff
                        ? "{\"action\":\"TURN_OFF\"}"
                        : $"{{\"action\":\"SET_BRIGHTNESS\",\"brightness\":{brightness}}}";
                    _ = _mqtt.SendCommandAsync(l.DeviceUid!, turnOff ? "TURN_OFF" : "SET_BRIGHTNESS", payload);
                }
                catch { /* ignore mqtt errors */ }
            }
        }

        db.ControlLogs.Add(new ControlLog
        {
            AreaId = s.AreaId,
            Action = turnOff ? "TURN_OFF" : "TURN_ON",
            Detail = $"策略[{s.Name}] {(isEnd ? "结束恢复" : "开始执行")} 亮度{brightness}%",
            StrategyId = s.Id,
            Result = 1,
            Remark = $"影响 {lights.Count} 盏"
        });

        _logger.LogInformation("[StrategyScheduler] strategy {Id} {Phase}: {Count} lights {Action}",
            s.Id, isEnd ? "end" : "start", lights.Count, turnOff ? "OFF" : "ON");
    }
}
