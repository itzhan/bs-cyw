package com.smartlight.config;

import lombok.extern.slf4j.Slf4j;
import org.eclipse.paho.client.mqttv3.MqttClient;
import org.eclipse.paho.client.mqttv3.MqttConnectOptions;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.eclipse.paho.client.mqttv3.persist.MemoryPersistence;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * MQTT 客户端配置 - 连接 IoT 设备通信 broker
 * 采用 Eclipse Paho MQTT v3 客户端
 */
@Slf4j
@Configuration
public class MqttConfig {

    @Value("${mqtt.broker-url:tcp://localhost:1883}")
    private String brokerUrl;

    @Value("${mqtt.client-id:smart-streetlight-server}")
    private String clientId;

    @Value("${mqtt.username:}")
    private String username;

    @Value("${mqtt.password:}")
    private String password;

    @Value("${mqtt.enabled:false}")
    private boolean enabled;

    /**
     * 创建 MQTT 客户端 Bean，连接失败时优雅降级（不影响其他功能）
     */
    @Bean
    public MqttClient mqttClient() {
        if (!enabled) {
            log.info("[MQTT] MQTT 已禁用，跳过连接（mqtt.enabled=false）");
            return null;
        }
        try {
            MqttClient client = new MqttClient(brokerUrl, clientId, new MemoryPersistence());
            MqttConnectOptions options = new MqttConnectOptions();
            options.setCleanSession(true);
            options.setAutomaticReconnect(true);
            options.setConnectionTimeout(10);
            options.setKeepAliveInterval(60);
            if (username != null && !username.isEmpty()) {
                options.setUserName(username);
                options.setPassword(password.toCharArray());
            }
            client.connect(options);
            log.info("[MQTT] 成功连接到 broker: {}", brokerUrl);
            return client;
        } catch (MqttException e) {
            log.warn("[MQTT] 连接 broker 失败: {}，系统将以离线模式运行", e.getMessage());
            return null;
        }
    }
}
