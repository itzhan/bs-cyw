package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.dto.UserUpdateDTO;
import com.smartlight.entity.Role;
import com.smartlight.entity.User;
import com.smartlight.repository.RoleRepository;
import com.smartlight.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;

import java.util.*;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/users")
@RequiredArgsConstructor
public class UserController {

    private final UserRepository userRepository;
    private final RoleRepository roleRepository;
    private final PasswordEncoder passwordEncoder;

    @GetMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) String keyword) {
        Page<User> p = userRepository.search(keyword, PageRequest.of(page - 1, size, Sort.by("id").descending()));
        List<Map<String, Object>> records = p.getContent().stream().map(this::toMap).collect(Collectors.toList());
        return Result.success(new PageResult<>(records, p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> detail(@PathVariable Long id) {
        User user = userRepository.findById(id).orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("用户不存在"));
        return Result.success(toMap(user));
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> update(@PathVariable Long id, @RequestBody UserUpdateDTO dto) {
        User user = userRepository.findById(id).orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("用户不存在"));
        if (dto.getRealName() != null) user.setRealName(dto.getRealName());
        if (dto.getPhone() != null) user.setPhone(dto.getPhone());
        if (dto.getEmail() != null) user.setEmail(dto.getEmail());
        if (dto.getAvatar() != null) user.setAvatar(dto.getAvatar());
        if (dto.getStatus() != null) user.setStatus(dto.getStatus());
        if (dto.getRoleIds() != null) {
            Set<Role> roles = new HashSet<>();
            for (Long roleId : dto.getRoleIds()) {
                roleRepository.findById(roleId).ifPresent(roles::add);
            }
            user.setRoles(roles);
        }
        userRepository.save(user);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        userRepository.deleteById(id);
        return Result.success();
    }

    @PutMapping("/{id}/reset-password")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> resetPassword(@PathVariable Long id) {
        User user = userRepository.findById(id).orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("用户不存在"));
        user.setPassword(passwordEncoder.encode("123456"));
        userRepository.save(user);
        return Result.success("密码已重置为123456", null);
    }

    private Map<String, Object> toMap(User u) {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("id", u.getId());
        m.put("username", u.getUsername());
        m.put("realName", u.getRealName());
        m.put("phone", u.getPhone());
        m.put("email", u.getEmail());
        m.put("avatar", u.getAvatar());
        m.put("status", u.getStatus());
        m.put("lastLoginTime", u.getLastLoginTime());
        m.put("roles", u.getRoles().stream().map(r -> {
            Map<String, Object> rm = new HashMap<>();
            rm.put("id", r.getId());
            rm.put("name", r.getName());
            rm.put("description", r.getDescription());
            return rm;
        }).collect(Collectors.toList()));
        m.put("createdAt", u.getCreatedAt());
        return m;
    }
}
