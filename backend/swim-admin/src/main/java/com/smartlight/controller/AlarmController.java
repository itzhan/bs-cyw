package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.dto.AlarmHandleDTO;
import com.smartlight.entity.Alarm;
import com.smartlight.entity.User;
import com.smartlight.repository.AlarmRepository;
import com.smartlight.repository.UserRepository;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;

@RestController
@RequestMapping("/api/alarms")
@RequiredArgsConstructor
public class AlarmController {

    private final AlarmRepository alarmRepository;
    private final UserRepository userRepository;

    @GetMapping
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Integer type,
                          @RequestParam(required = false) Integer level,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) Long areaId,
                          @RequestParam(required = false) String keyword) {
        Page<Alarm> p = alarmRepository.search(type, level, status, areaId, keyword,
                PageRequest.of(page - 1, size, Sort.by("alarmTime").descending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(alarmRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("告警不存在")));
    }

    @PutMapping("/{id}/handle")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> handle(@PathVariable Long id, @RequestBody @Valid AlarmHandleDTO dto) {
        Alarm alarm = alarmRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("告警不存在"));
        alarm.setStatus(dto.getStatus());
        alarm.setHandleRemark(dto.getHandleRemark());
        alarm.setHandleTime(LocalDateTime.now());
        String username = SecurityContextHolder.getContext().getAuthentication().getName();
        User handler = userRepository.findByUsername(username).orElse(null);
        if (handler != null) alarm.setHandlerId(handler.getId());
        alarmRepository.save(alarm);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        alarmRepository.deleteById(id);
        return Result.success();
    }
}
