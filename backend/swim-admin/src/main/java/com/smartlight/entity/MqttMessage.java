package com.smartlight.entity;

import jakarta.persistence.*;
import lombok.Data;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "mqtt_message")
public class MqttMessage {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "device_uid")
    private String deviceUid;

    private String topic;

    @Column(columnDefinition = "TEXT")
    private String payload;

    /** 1-上行(设备→平台) 2-下行(平台→设备) */
    private Integer direction;

    /** 0-失败 1-成功 */
    private Integer status;

    @Column(name = "created_at", updatable = false)
    private LocalDateTime createdAt;

    @PrePersist
    public void prePersist() {
        if (createdAt == null) createdAt = LocalDateTime.now();
        if (status == null) status = 1;
        if (direction == null) direction = 1;
    }
}
