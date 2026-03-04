package com.smartlight.controller;

import com.smartlight.common.Result;
import com.smartlight.entity.Area;
import com.smartlight.repository.AreaRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/areas")
@RequiredArgsConstructor
public class AreaController {

    private final AreaRepository areaRepository;

    @GetMapping
    public Result<?> list() {
        return Result.success(areaRepository.findAll());
    }

    @GetMapping("/tree")
    public Result<?> tree() {
        return Result.success(areaRepository.findByParentIdOrderBySortOrder(0L));
    }

    @GetMapping("/children/{parentId}")
    public Result<?> children(@PathVariable Long parentId) {
        return Result.success(areaRepository.findByParentIdOrderBySortOrder(parentId));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(areaRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("区域不存在")));
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> create(@RequestBody Area area) {
        if (areaRepository.existsByCode(area.getCode())) {
            return Result.error(400, "区域编码已存在");
        }
        areaRepository.save(area);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> update(@PathVariable Long id, @RequestBody Area dto) {
        Area entity = areaRepository.findById(id).orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("区域不存在"));
        if (dto.getName() != null) entity.setName(dto.getName());
        if (dto.getDescription() != null) entity.setDescription(dto.getDescription());
        if (dto.getSortOrder() != null) entity.setSortOrder(dto.getSortOrder());
        if (dto.getStatus() != null) entity.setStatus(dto.getStatus());
        areaRepository.save(entity);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        List<Area> children = areaRepository.findByParentIdOrderBySortOrder(id);
        if (!children.isEmpty()) {
            return Result.error(400, "该区域下存在子区域，无法删除");
        }
        areaRepository.deleteById(id);
        return Result.success();
    }
}
