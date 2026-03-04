package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.entity.Cabinet;
import com.smartlight.repository.CabinetRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/cabinets")
@RequiredArgsConstructor
public class CabinetController {

    private final CabinetRepository cabinetRepository;

    @GetMapping
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Long areaId,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) String keyword) {
        Page<Cabinet> p = cabinetRepository.search(areaId, status, keyword,
                PageRequest.of(page - 1, size, Sort.by("id").ascending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(cabinetRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("电柜不存在")));
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> create(@RequestBody Cabinet cabinet) {
        cabinetRepository.save(cabinet);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> update(@PathVariable Long id, @RequestBody Cabinet dto) {
        Cabinet entity = cabinetRepository.findById(id).orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("电柜不存在"));
        if (dto.getName() != null) entity.setName(dto.getName());
        if (dto.getAddress() != null) entity.setAddress(dto.getAddress());
        if (dto.getLongitude() != null) entity.setLongitude(dto.getLongitude());
        if (dto.getLatitude() != null) entity.setLatitude(dto.getLatitude());
        if (dto.getCapacity() != null) entity.setCapacity(dto.getCapacity());
        if (dto.getStatus() != null) entity.setStatus(dto.getStatus());
        cabinetRepository.save(entity);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        cabinetRepository.deleteById(id);
        return Result.success();
    }
}
