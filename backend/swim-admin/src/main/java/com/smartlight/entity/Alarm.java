package com.smartlight.entity;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;
import lombok.Data;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "alarm")
public class Alarm {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "alarm_code", nullable = false, unique = true, length = 50)
    private String alarmCode;

    @Column(nullable = false)
    private Integer type;

    @Column(nullable = false)
    private Integer level = 2;

    @Column(name = "streetlight_id")
    private Long streetlightId;

    @Column(name = "cabinet_id")
    private Long cabinetId;

    @Column(name = "area_id")
    private Long areaId;

    @Column(nullable = false, length = 200)
    private String title;

    @Column(columnDefinition = "TEXT")
    private String description;

    @Column(nullable = false)
    private Integer status = 0;

    @Column(name = "handler_id")
    private Long handlerId;

    @Column(name = "handle_time")
    private LocalDateTime handleTime;

    @Column(name = "handle_remark", length = 500)
    private String handleRemark;

    @Column(name = "alarm_time", nullable = false)
    private LocalDateTime alarmTime;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "streetlight_id", insertable = false, updatable = false)
    @JsonIgnoreProperties({"hibernateLazyInitializer", "handler", "area", "cabinet"})
    private Streetlight streetlight;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "area_id", insertable = false, updatable = false)
    @JsonIgnoreProperties({"hibernateLazyInitializer", "handler"})
    private Area area;

    @PrePersist
    protected void onCreate() {
        createdAt = LocalDateTime.now();
        updatedAt = LocalDateTime.now();
        if (alarmTime == null) alarmTime = LocalDateTime.now();
    }

    @PreUpdate
    protected void onUpdate() { updatedAt = LocalDateTime.now(); }
}
