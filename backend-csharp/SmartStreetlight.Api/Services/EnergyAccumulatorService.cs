using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Services;

public class EnergyAccumulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnergyAccumulatorService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private const string RateKey = "energy.kwh_per_minute";
    private const decimal DefaultKwhPerMinute = 0.6m;

    public EnergyAccumulatorService(IServiceScopeFactory scopeFactory, ILogger<EnergyAccumulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[EnergyAccumulator] started, interval 60s, rate read from system_setting[{Key}]", RateKey);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AccumulateOnceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[EnergyAccumulator] tick failed: {Message}", ex.Message);
            }
            try { await Task.Delay(Interval, stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task AccumulateOnceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var today = DateOnly.FromDateTime(DateTime.Now);

        var kwhPerMinute = await LoadRateAsync(db);

        var activeLights = await db.Streetlights
            .AsNoTracking()
            .Where(s => s.LightStatus == 1)
            .Select(s => new { s.Id, s.CabinetId, s.AreaId })
            .ToListAsync();

        if (activeLights.Count == 0) return;

        var lightIds = activeLights.Select(light => light.Id).ToList();
        var existingRecords = await db.EnergyRecords
            .Where(r => r.RecordDate == today && r.StreetlightId.HasValue && lightIds.Contains(r.StreetlightId.Value))
            .ToListAsync();
        var recordMap = existingRecords
            .Where(r => r.StreetlightId.HasValue)
            .ToDictionary(r => r.StreetlightId!.Value);

        foreach (var light in activeLights)
        {
            if (!recordMap.TryGetValue(light.Id, out var rec))
            {
                rec = new EnergyRecord
                {
                    StreetlightId = light.Id,
                    CabinetId = light.CabinetId,
                    AreaId = light.AreaId,
                    RecordDate = today,
                    EnergyKwh = kwhPerMinute,
                    RunningMinutes = 1
                };
                db.EnergyRecords.Add(rec);
                recordMap[light.Id] = rec;
            }
            else
            {
                rec.EnergyKwh += kwhPerMinute;
                rec.RunningMinutes += 1;
            }
        }

        await db.SaveChangesAsync();
        _logger.LogDebug("[EnergyAccumulator] +{Kwh}kWh across {Count} lights (rate={Rate})", kwhPerMinute * activeLights.Count, activeLights.Count, kwhPerMinute);
    }

    private async Task<decimal> LoadRateAsync(AppDbContext db)
    {
        try
        {
            var setting = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == RateKey);
            if (setting == null) return DefaultKwhPerMinute;
            if (decimal.TryParse(setting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate) && rate >= 0)
                return rate;
            _logger.LogWarning("[EnergyAccumulator] invalid rate value '{Value}', falling back to default {Default}", setting.Value, DefaultKwhPerMinute);
            return DefaultKwhPerMinute;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[EnergyAccumulator] could not read rate setting ({Msg}), using default {Default}", ex.Message, DefaultKwhPerMinute);
            return DefaultKwhPerMinute;
        }
    }
}
