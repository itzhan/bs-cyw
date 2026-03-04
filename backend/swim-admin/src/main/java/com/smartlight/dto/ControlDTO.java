package com.smartlight.dto;

import lombok.Data;
import java.util.List;

@Data
public class ControlDTO {
    private List<Long> streetlightIds;
    private Long areaId;
    private String action;
    private Integer brightness;
    private String remark;
}
