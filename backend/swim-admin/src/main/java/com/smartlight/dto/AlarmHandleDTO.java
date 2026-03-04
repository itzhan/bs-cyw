package com.smartlight.dto;

import jakarta.validation.constraints.NotNull;
import lombok.Data;

@Data
public class AlarmHandleDTO {
    @NotNull(message = "状态不能为空")
    private Integer status;
    private String handleRemark;
}
