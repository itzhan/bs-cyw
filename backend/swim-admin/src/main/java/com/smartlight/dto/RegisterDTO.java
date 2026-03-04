package com.smartlight.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import lombok.Data;

@Data
public class RegisterDTO {
    @NotBlank(message = "用户名不能为空")
    @Size(min = 3, max = 50, message = "用户名长度3-50位")
    private String username;
    @NotBlank(message = "密码不能为空")
    @Size(min = 6, max = 30, message = "密码长度6-30位")
    private String password;
    private String realName;
    private String phone;
    private String email;
}
