package com.smartlight.entity;

import jakarta.persistence.*;
import lombok.Data;
import java.math.BigDecimal;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "repair_report")
public class RepairReport {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "report_no", nullable = false, unique = true, length = 50)
    private String reportNo;

    @Column(name = "reporter_id")
    private Long reporterId;

    @Column(name = "reporter_name", length = 50)
    private String reporterName;

    @Column(name = "reporter_phone", length = 20)
    private String reporterPhone;

    @Column(name = "streetlight_id")
    private Long streetlightId;

    @Column(length = 500)
    private String address;

    @Column(precision = 10, scale = 7)
    private BigDecimal longitude;

    @Column(precision = 10, scale = 7)
    private BigDecimal latitude;

    @Column(columnDefinition = "TEXT")
    private String description;

    @Column(length = 2000)
    private String images;

    @Column(nullable = false)
    private Integer status = 0;

    @Column(name = "work_order_id")
    private Long workOrderId;

    @Column(length = 500)
    private String reply;

    @Column(name = "created_at", nullable = false, updatable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    @PrePersist
    protected void onCreate() { createdAt = LocalDateTime.now(); updatedAt = LocalDateTime.now(); }

    @PreUpdate
    protected void onUpdate() { updatedAt = LocalDateTime.now(); }
}
