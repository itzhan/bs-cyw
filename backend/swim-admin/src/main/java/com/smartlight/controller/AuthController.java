package com.smartlight.controller;

import com.smartlight.common.Result;
import com.smartlight.dto.LoginDTO;
import com.smartlight.dto.PasswordDTO;
import com.smartlight.dto.RegisterDTO;
import com.smartlight.entity.Role;
import com.smartlight.entity.User;
import com.smartlight.repository.RoleRepository;
import com.smartlight.repository.UserRepository;
import com.smartlight.util.JwtUtil;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/auth")
@RequiredArgsConstructor
public class AuthController {

    private final AuthenticationManager authenticationManager;
    private final UserRepository userRepository;
    private final RoleRepository roleRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtUtil jwtUtil;

    @PostMapping("/login")
    public Result<?> login(@RequestBody @Valid LoginDTO dto) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(dto.getUsername(), dto.getPassword()));
        User user = userRepository.findByUsername(dto.getUsername()).orElseThrow();
        user.setLastLoginTime(LocalDateTime.now());
        userRepository.save(user);
        Map<String, Object> claims = new HashMap<>();
        claims.put("userId", user.getId());
        claims.put("roles", user.getRoles().stream().map(Role::getName).collect(Collectors.toList()));
        String token = jwtUtil.generateToken(user.getUsername(), claims);
        Map<String, Object> result = new HashMap<>();
        result.put("token", token);
        result.put("userId", user.getId());
        result.put("username", user.getUsername());
        result.put("realName", user.getRealName());
        result.put("avatar", user.getAvatar());
        result.put("roles", user.getRoles().stream().map(Role::getName).collect(Collectors.toList()));
        return Result.success(result);
    }

    @PostMapping("/register")
    public Result<?> register(@RequestBody @Valid RegisterDTO dto) {
        if (userRepository.existsByUsername(dto.getUsername())) {
            return Result.error(400, "用户名已存在");
        }
        User user = new User();
        user.setUsername(dto.getUsername());
        user.setPassword(passwordEncoder.encode(dto.getPassword()));
        user.setRealName(dto.getRealName());
        user.setPhone(dto.getPhone());
        user.setEmail(dto.getEmail());
        user.setStatus(1);
        Role userRole = roleRepository.findByName("USER").orElseThrow();
        user.setRoles(new HashSet<>(Collections.singletonList(userRole)));
        userRepository.save(user);
        return Result.success("注册成功", null);
    }

    @GetMapping("/info")
    public Result<?> info() {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        User user = userRepository.findByUsername(auth.getName()).orElseThrow();
        Map<String, Object> result = new HashMap<>();
        result.put("userId", user.getId());
        result.put("username", user.getUsername());
        result.put("realName", user.getRealName());
        result.put("phone", user.getPhone());
        result.put("email", user.getEmail());
        result.put("avatar", user.getAvatar());
        result.put("roles", user.getRoles().stream().map(Role::getName).collect(Collectors.toList()));
        return Result.success(result);
    }

    @PutMapping("/password")
    public Result<?> changePassword(@RequestBody @Valid PasswordDTO dto) {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        User user = userRepository.findByUsername(auth.getName()).orElseThrow();
        if (!passwordEncoder.matches(dto.getOldPassword(), user.getPassword())) {
            return Result.error(400, "旧密码错误");
        }
        user.setPassword(passwordEncoder.encode(dto.getNewPassword()));
        userRepository.save(user);
        return Result.success("密码修改成功", null);
    }

    @PutMapping("/profile")
    public Result<?> updateProfile(@RequestBody Map<String, String> body) {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        User user = userRepository.findByUsername(auth.getName()).orElseThrow();
        if (body.containsKey("realName")) user.setRealName(body.get("realName"));
        if (body.containsKey("phone")) user.setPhone(body.get("phone"));
        if (body.containsKey("email")) user.setEmail(body.get("email"));
        if (body.containsKey("avatar")) user.setAvatar(body.get("avatar"));
        userRepository.save(user);
        return Result.success("个人信息更新成功", null);
    }
}
