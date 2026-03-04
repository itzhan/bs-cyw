package com.smartlight.entity;

import jakarta.persistence.*;
import lombok.Data;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "control_log")
public class ControlLog {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "streetlight_id")
    private Long streetlightId;

    @Column(name = "area_id")
    private Long areaId;

    @Column(nullable = false, length = 50)
    private String action;

    @Column(length = 500)
    private String detail;

    @Column(name = "operator_id")
    private Long operatorId;

    @Column(name = "strategy_id")
    private Long strategyId;

    @Column(nullable = false)
    private Integer result = 1;

    @Column(length = 500)
    private String remark;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @PrePersist
    protected void onCreate() { createdAt = LocalDateTime.now(); }
}
