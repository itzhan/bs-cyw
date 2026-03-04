package com.smartlight.entity;

import jakarta.persistence.*;
import lombok.Data;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "energy_record")
public class EnergyRecord {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "streetlight_id")
    private Long streetlightId;

    @Column(name = "cabinet_id")
    private Long cabinetId;

    @Column(name = "area_id")
    private Long areaId;

    @Column(name = "record_date", nullable = false)
    private LocalDate recordDate;

    @Column(name = "energy_kwh", nullable = false, precision = 10, scale = 3)
    private BigDecimal energyKwh = BigDecimal.ZERO;

    @Column(name = "running_minutes", nullable = false)
    private Integer runningMinutes = 0;

    @Column(name = "avg_power", precision = 8, scale = 2)
    private BigDecimal avgPower;

    @Column(name = "peak_power", precision = 8, scale = 2)
    private BigDecimal peakPower;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @PrePersist
    protected void onCreate() { createdAt = LocalDateTime.now(); }
}
