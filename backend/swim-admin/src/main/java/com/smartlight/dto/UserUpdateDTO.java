package com.smartlight.dto;

import lombok.Data;

@Data
public class UserUpdateDTO {
    private String realName;
    private String phone;
    private String email;
    private String avatar;
    private Integer status;
    private Long[] roleIds;
}
