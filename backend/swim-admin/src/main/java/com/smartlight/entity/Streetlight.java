package com.smartlight.entity;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;
import lombok.Data;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "streetlight")
public class Streetlight {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false, unique = true, length = 50)
    private String code;

    @Column(name = "device_uid", length = 100)
    private String deviceUid;

    @Column(nullable = false, length = 100)
    private String name;

    @Column(name = "area_id", nullable = false)
    private Long areaId;

    @Column(name = "cabinet_id")
    private Long cabinetId;

    @Column(length = 500)
    private String address;

    @Column(precision = 10, scale = 7)
    private BigDecimal longitude;

    @Column(precision = 10, scale = 7)
    private BigDecimal latitude;

    @Column(name = "lamp_type", length = 50)
    private String lampType;

    @Column(name = "hardware_model", length = 100)
    private String hardwareModel;

    @Column(name = "electrical_params", length = 500)
    private String electricalParams;

    @Column(name = "protection_rating", length = 20)
    private String protectionRating;

    private Integer power;

    @Column(precision = 5, scale = 2)
    private BigDecimal height;

    private Integer brightness = 100;

    @Column(name = "online_status", nullable = false)
    private Integer onlineStatus = 1;

    @Column(name = "light_status", nullable = false)
    private Integer lightStatus = 0;

    @Column(name = "device_status", nullable = false)
    private Integer deviceStatus = 1;

    @Column(name = "install_date")
    private LocalDate installDate;

    @Column(name = "last_maintain_date")
    private LocalDate lastMaintainDate;

    @Column(precision = 6, scale = 2)
    private BigDecimal voltage;

    @Column(name = "current_val", precision = 6, scale = 3)
    private BigDecimal currentVal;

    @Column(precision = 5, scale = 2)
    private BigDecimal temperature;

    @Column(name = "running_hours")
    private Integer runningHours = 0;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "area_id", insertable = false, updatable = false)
    @JsonIgnoreProperties({"hibernateLazyInitializer", "handler"})
    private Area area;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "cabinet_id", insertable = false, updatable = false)
    @JsonIgnoreProperties({"hibernateLazyInitializer", "handler", "area"})
    private Cabinet cabinet;

    @PrePersist
    protected void onCreate() { createdAt = LocalDateTime.now(); updatedAt = LocalDateTime.now(); }

    @PreUpdate
    protected void onUpdate() { updatedAt = LocalDateTime.now(); }
}
