package com.smartlight.service;

import com.smartlight.entity.MqttMessage;
import com.smartlight.repository.MqttMessageRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.eclipse.paho.client.mqttv3.MqttClient;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.springframework.lang.Nullable;
import org.springframework.stereotype.Service;

/**
 * MQTT 消息发布服务 - 向设备下发控制指令
 */
@Slf4j
@Service
@RequiredArgsConstructor
public class MqttPublishService {

    @Nullable
    private final MqttClient mqttClient;
    private final MqttMessageRepository mqttMessageRepository;

    /**
     * 向指定设备发送控制指令
     */
    public boolean sendCommand(String deviceUid, String action, String payload) {
        String topic = "streetlight/" + deviceUid + "/control";
        MqttMessage record = new MqttMessage();
        record.setDeviceUid(deviceUid);
        record.setTopic(topic);
        record.setPayload(payload);
        record.setDirection(2); // 下行

        if (mqttClient == null || !mqttClient.isConnected()) {
            log.warn("[MQTT] 客户端未连接，指令已记录但未实际发送: {} -> {}", topic, payload);
            record.setStatus(0);
            mqttMessageRepository.save(record);
            return false;
        }

        try {
            org.eclipse.paho.client.mqttv3.MqttMessage msg = new org.eclipse.paho.client.mqttv3.MqttMessage(payload.getBytes());
            msg.setQos(1);
            mqttClient.publish(topic, msg);
            record.setStatus(1);
            mqttMessageRepository.save(record);
            log.info("[MQTT] 指令下发成功: {} -> {}", topic, payload);
            return true;
        } catch (MqttException e) {
            log.error("[MQTT] 指令下发失败: {}", e.getMessage());
            record.setStatus(0);
            mqttMessageRepository.save(record);
            return false;
        }
    }

    /**
     * 检查 MQTT 连接状态
     */
    public boolean isConnected() {
        return mqttClient != null && mqttClient.isConnected();
    }

    /**
     * 获取 broker 信息
     */
    public String getBrokerInfo() {
        if (mqttClient == null) return "未配置";
        return mqttClient.getServerURI();
    }
}
