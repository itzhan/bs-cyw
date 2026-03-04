package com.smartlight.dto;

import lombok.Data;
import java.math.BigDecimal;
import java.time.LocalDateTime;

@Data
public class WorkOrderDTO {
    private Long alarmId;
    private Long streetlightId;
    private Long areaId;
    private String title;
    private String description;
    private Integer priority;
    private Long assigneeId;
    private LocalDateTime expectedFinish;
    private String repairContent;
    private BigDecimal repairCost;
}
