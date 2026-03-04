package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.entity.ControlStrategy;
import com.smartlight.repository.ControlStrategyRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/strategies")
@RequiredArgsConstructor
public class ControlStrategyController {

    private final ControlStrategyRepository strategyRepository;

    @GetMapping
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Integer type,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) String keyword) {
        Page<ControlStrategy> p = strategyRepository.search(type, status, keyword,
                PageRequest.of(page - 1, size, Sort.by("priority").descending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(strategyRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("策略不存在")));
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> create(@RequestBody ControlStrategy strategy) {
        strategyRepository.save(strategy);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> update(@PathVariable Long id, @RequestBody ControlStrategy dto) {
        ControlStrategy entity = strategyRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("策略不存在"));
        if (dto.getName() != null) entity.setName(dto.getName());
        if (dto.getType() != null) entity.setType(dto.getType());
        if (dto.getDescription() != null) entity.setDescription(dto.getDescription());
        if (dto.getStartTime() != null) entity.setStartTime(dto.getStartTime());
        if (dto.getEndTime() != null) entity.setEndTime(dto.getEndTime());
        if (dto.getBrightness() != null) entity.setBrightness(dto.getBrightness());
        if (dto.getLightThreshold() != null) entity.setLightThreshold(dto.getLightThreshold());
        if (dto.getEffectiveStart() != null) entity.setEffectiveStart(dto.getEffectiveStart());
        if (dto.getEffectiveEnd() != null) entity.setEffectiveEnd(dto.getEffectiveEnd());
        if (dto.getStatus() != null) entity.setStatus(dto.getStatus());
        if (dto.getPriority() != null) entity.setPriority(dto.getPriority());
        if (dto.getAreaId() != null) entity.setAreaId(dto.getAreaId());
        strategyRepository.save(entity);
        return Result.success();
    }

    @PutMapping("/{id}/toggle")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> toggle(@PathVariable Long id) {
        ControlStrategy entity = strategyRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("策略不存在"));
        entity.setStatus(entity.getStatus() == 1 ? 0 : 1);
        strategyRepository.save(entity);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        strategyRepository.deleteById(id);
        return Result.success();
    }
}
