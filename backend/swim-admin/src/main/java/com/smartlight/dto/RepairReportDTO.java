package com.smartlight.dto;

import lombok.Data;
import java.math.BigDecimal;

@Data
public class RepairReportDTO {
    private String reporterName;
    private String reporterPhone;
    private Long streetlightId;
    private String address;
    private BigDecimal longitude;
    private BigDecimal latitude;
    private String description;
    private String images;
}
