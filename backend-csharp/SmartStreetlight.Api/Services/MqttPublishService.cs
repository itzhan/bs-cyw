using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;
using System.Text;
using System.Text.Json;

namespace SmartStreetlight.Api.Services;

public class MqttPublishService
{
    private IMqttClient? _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttPublishService> _logger;
    private string _brokerInfo = "未配置";

    public MqttPublishService(IServiceScopeFactory scopeFactory, ILogger<MqttPublishService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(IConfiguration config)
    {
        var enabled = config.GetValue<bool>("Mqtt:Enabled");
        if (!enabled)
        {
            _logger.LogInformation("[MQTT] MQTT is disabled in configuration");
            return;
        }

        var broker = config["Mqtt:BrokerUrl"] ?? "localhost";
        var port = config.GetValue<int>("Mqtt:BrokerPort", 1883);
        var clientId = config["Mqtt:ClientId"] ?? "smart-streetlight-csharp";
        _brokerInfo = $"tcp://{broker}:{port}";

        try
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _client.ApplicationMessageReceivedAsync += HandleIncomingMessageAsync;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            await _client.ConnectAsync(options);
            await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(filter =>
                {
                    filter.WithTopic("streetlight/+/status");
                    filter.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);
                })
                .WithTopicFilter(filter =>
                {
                    filter.WithTopic("streetlight/+/alarm");
                    filter.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);
                })
                .Build());

            _logger.LogInformation("[MQTT] Connected to {Broker}", _brokerInfo);
            _logger.LogInformation("[MQTT] Subscribed topics: streetlight/+/status, streetlight/+/alarm");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[MQTT] Failed to connect: {Message}", ex.Message);
        }
    }

    public bool IsConnected => _client?.IsConnected ?? false;
    public string BrokerInfo => _brokerInfo;

    public async Task<bool> SendCommandAsync(string deviceUid, string action, string payload)
    {
        var topic = $"streetlight/{deviceUid}/control";

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var record = new MqttMessage
        {
            DeviceUid = deviceUid,
            Topic = topic,
            Payload = payload,
            Direction = 2, // Downlink
            Status = 0
        };

        if (_client == null || !_client.IsConnected)
        {
            _logger.LogWarning("[MQTT] Client not connected, command recorded but not sent: {Topic}", topic);
            db.MqttMessages.Add(record);
            await db.SaveChangesAsync();
            return false;
        }

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _client.PublishAsync(message);
            record.Status = 1;
            db.MqttMessages.Add(record);
            await db.SaveChangesAsync();
            _logger.LogInformation("[MQTT] Command sent: {Topic} -> {Payload}", topic, payload);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("[MQTT] Failed to send: {Message}", ex.Message);
            db.MqttMessages.Add(record);
            await db.SaveChangesAsync();
            return false;
        }
    }

    private async Task HandleIncomingMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic ?? "";
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        var deviceUid = GetDeviceUidFromTopic(topic);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var record = new MqttMessage
        {
            DeviceUid = deviceUid,
            Topic = topic,
            Payload = payload,
            Direction = 1, // Uplink
            Status = 1
        };

        try
        {
            if (topic.EndsWith("/status", StringComparison.OrdinalIgnoreCase))
            {
                var parsedUid = await ApplyStatusToStreetlightAsync(db, payload, deviceUid);
                if (!string.IsNullOrWhiteSpace(parsedUid))
                {
                    record.DeviceUid = parsedUid;
                }
            }
            else if (topic.EndsWith("/alarm", StringComparison.OrdinalIgnoreCase))
            {
                var parsedUid = await SaveAlarmAsync(db, payload, deviceUid);
                if (!string.IsNullOrWhiteSpace(parsedUid))
                {
                    record.DeviceUid = parsedUid;
                }
            }
        }
        catch (Exception ex)
        {
            record.Status = 0;
            _logger.LogWarning("[MQTT] Failed handling uplink message: {Topic}, {Message}", topic, ex.Message);
        }

        db.MqttMessages.Add(record);
        await db.SaveChangesAsync();
    }

    private static string? GetDeviceUidFromTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return null;
        }

        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 && parts[0] == "streetlight")
        {
            return parts[1];
        }

        return null;
    }

    private async Task<string?> ApplyStatusToStreetlightAsync(AppDbContext db, string payload, string? fallbackDeviceUid)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var deviceUid = fallbackDeviceUid;
        if (root.TryGetProperty("deviceUid", out var uidProp))
        {
            deviceUid = uidProp.GetString();
        }

        if (string.IsNullOrWhiteSpace(deviceUid))
        {
            return null;
        }

        var light = await db.Streetlights.FirstOrDefaultAsync(s => s.DeviceUid == deviceUid);
        if (light == null)
        {
            return deviceUid;
        }

        if (root.TryGetProperty("voltage", out var voltage) && voltage.ValueKind == JsonValueKind.Number)
        {
            light.Voltage = (decimal)voltage.GetDouble();
        }
        if (root.TryGetProperty("current", out var current) && current.ValueKind == JsonValueKind.Number)
        {
            light.CurrentVal = (decimal)current.GetDouble();
        }
        if (root.TryGetProperty("temperature", out var temperature) && temperature.ValueKind == JsonValueKind.Number)
        {
            light.Temperature = (decimal)temperature.GetDouble();
        }
        if (root.TryGetProperty("brightness", out var brightness) && brightness.ValueKind == JsonValueKind.Number)
        {
            light.Brightness = brightness.GetInt32();
        }
        if (root.TryGetProperty("lightStatus", out var lightStatus) && lightStatus.ValueKind == JsonValueKind.Number)
        {
            light.LightStatus = lightStatus.GetInt32();
        }
        if (root.TryGetProperty("onlineStatus", out var onlineStatus) && onlineStatus.ValueKind == JsonValueKind.Number)
        {
            light.OnlineStatus = onlineStatus.GetInt32();
        }
        else
        {
            // Status uplink from device implies device is online.
            light.OnlineStatus = 1;
        }
        if (root.TryGetProperty("deviceStatus", out var deviceStatus) && deviceStatus.ValueKind == JsonValueKind.Number)
        {
            light.DeviceStatus = deviceStatus.GetInt32();
        }
        if (root.TryGetProperty("runningHours", out var runningHours) && runningHours.ValueKind == JsonValueKind.Number)
        {
            light.RunningHours = runningHours.GetInt32();
        }
        if (root.TryGetProperty("hardwareModel", out var hardwareModel) && hardwareModel.ValueKind == JsonValueKind.String)
        {
            light.HardwareModel = hardwareModel.GetString();
        }
        if (root.TryGetProperty("protectionRating", out var protectionRating) && protectionRating.ValueKind == JsonValueKind.String)
        {
            light.ProtectionRating = protectionRating.GetString();
        }
        if (root.TryGetProperty("longitude", out var longitude) && longitude.ValueKind == JsonValueKind.Number)
        {
            light.Longitude = (decimal)longitude.GetDouble();
        }
        if (root.TryGetProperty("latitude", out var latitude) && latitude.ValueKind == JsonValueKind.Number)
        {
            light.Latitude = (decimal)latitude.GetDouble();
        }

        return deviceUid;
    }

    private async Task<string?> SaveAlarmAsync(AppDbContext db, string payload, string? fallbackDeviceUid)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var deviceUid = fallbackDeviceUid;
        if (root.TryGetProperty("deviceUid", out var uidProp))
        {
            deviceUid = uidProp.GetString();
        }

        int alarmType = 8;
        if (root.TryGetProperty("alarmType", out var typeProp) && typeProp.ValueKind == JsonValueKind.Number)
        {
            alarmType = typeProp.GetInt32();
        }

        var message = root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String
            ? msgProp.GetString() ?? "设备上报告警"
            : "设备上报告警";

        Streetlight? light = null;
        if (!string.IsNullOrWhiteSpace(deviceUid))
        {
            light = await db.Streetlights.FirstOrDefaultAsync(s => s.DeviceUid == deviceUid);
            if (light != null)
            {
                light.DeviceStatus = 2;
                light.OnlineStatus = 1;
            }
        }

        var now = DateTime.Now;
        var alarm = new Alarm
        {
            AlarmCode = $"ALM{now:yyMMddHHmmssfff}{Random.Shared.Next(100, 999)}",
            Type = alarmType,
            Level = alarmType == 7 ? 4 : (alarmType == 6 ? 3 : 2),
            StreetlightId = light?.Id,
            AreaId = light?.AreaId,
            Title = $"{deviceUid ?? "未知设备"} 告警",
            Description = message,
            Status = 0,
            AlarmTime = now
        };
        db.Alarms.Add(alarm);

        return deviceUid;
    }
}
