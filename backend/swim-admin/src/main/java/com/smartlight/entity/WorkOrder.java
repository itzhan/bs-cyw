package com.smartlight.entity;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;
import lombok.Data;
import java.math.BigDecimal;
import java.time.LocalDateTime;

@Data
@Entity
@Table(name = "work_order")
public class WorkOrder {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "order_no", nullable = false, unique = true, length = 50)
    private String orderNo;

    @Column(name = "alarm_id")
    private Long alarmId;

    @Column(name = "streetlight_id")
    private Long streetlightId;

    @Column(name = "area_id")
    private Long areaId;

    @Column(nullable = false, length = 200)
    private String title;

    @Column(columnDefinition = "TEXT")
    private String description;

    @Column(nullable = false)
    private Integer priority = 2;

    @Column(nullable = false)
    private Integer status = 0;

    @Column(name = "assignee_id")
    private Long assigneeId;

    @Column(name = "reporter_id")
    private Long reporterId;

    @Column(name = "expected_finish")
    private LocalDateTime expectedFinish;

    @Column(name = "actual_finish")
    private LocalDateTime actualFinish;

    @Column(name = "repair_content", columnDefinition = "TEXT")
    private String repairContent;

    @Column(name = "repair_cost", precision = 10, scale = 2)
    private BigDecimal repairCost;

    @Column(length = 2000)
    private String images;

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
    protected void onCreate() { createdAt = LocalDateTime.now(); updatedAt = LocalDateTime.now(); }

    @PreUpdate
    protected void onUpdate() { updatedAt = LocalDateTime.now(); }
}
