using MQTTnet;
using MQTTnet.Client;
using SmartStreetlight.Api.Data;
using SmartStreetlight.Api.Models.Entities;

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

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            await _client.ConnectAsync(options);
            _logger.LogInformation("[MQTT] Connected to {Broker}", _brokerInfo);
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
}
