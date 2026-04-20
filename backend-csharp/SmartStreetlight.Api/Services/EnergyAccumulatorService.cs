using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Services;

public class EnergyAccumulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnergyAccumulatorService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private const decimal KwhPerMinute = 0.6m; // 0.01 kWh/s × 60s

    public EnergyAccumulatorService(IServiceScopeFactory scopeFactory, ILogger<EnergyAccumulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[EnergyAccumulator] started, interval 60s, rate 0.01kWh/s");
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

        var activeLights = await db.Streetlights
            .Where(s => s.LightStatus == 1)
            .Select(s => new { s.Id, s.CabinetId, s.AreaId })
            .ToListAsync();

        if (activeLights.Count == 0) return;

        foreach (var light in activeLights)
        {
            var rec = await db.EnergyRecords.FirstOrDefaultAsync(r =>
                r.StreetlightId == light.Id && r.RecordDate == today);
            if (rec == null)
            {
                db.EnergyRecords.Add(new EnergyRecord
                {
                    StreetlightId = light.Id,
                    CabinetId = light.CabinetId,
                    AreaId = light.AreaId,
                    RecordDate = today,
                    EnergyKwh = KwhPerMinute,
                    RunningMinutes = 1
                });
            }
            else
            {
                rec.EnergyKwh += KwhPerMinute;
                rec.RunningMinutes += 1;
            }
        }

        await db.SaveChangesAsync();
        _logger.LogDebug("[EnergyAccumulator] +{Kwh}kWh across {Count} lights", KwhPerMinute * activeLights.Count, activeLights.Count);
    }
}
