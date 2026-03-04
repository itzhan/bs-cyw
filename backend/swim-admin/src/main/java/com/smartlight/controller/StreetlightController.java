package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.entity.Streetlight;
import com.smartlight.repository.StreetlightRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/streetlights")
@RequiredArgsConstructor
public class StreetlightController {

    private final StreetlightRepository streetlightRepository;

    @GetMapping
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Long areaId,
                          @RequestParam(required = false) Long cabinetId,
                          @RequestParam(required = false) Integer onlineStatus,
                          @RequestParam(required = false) Integer deviceStatus,
                          @RequestParam(required = false) String keyword) {
        Page<Streetlight> p = streetlightRepository.search(areaId, cabinetId, onlineStatus, deviceStatus, keyword,
                PageRequest.of(page - 1, size, Sort.by("id").ascending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/all")
    public Result<?> all(@RequestParam(required = false) Long areaId) {
        List<Streetlight> list = areaId != null ? streetlightRepository.findByAreaId(areaId) : streetlightRepository.findAll();
        return Result.success(list);
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(streetlightRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("路灯不存在")));
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> create(@RequestBody Streetlight streetlight) {
        streetlightRepository.save(streetlight);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> update(@PathVariable Long id, @RequestBody Streetlight dto) {
        Streetlight entity = streetlightRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("路灯不存在"));
        if (dto.getName() != null) entity.setName(dto.getName());
        if (dto.getAddress() != null) entity.setAddress(dto.getAddress());
        if (dto.getLongitude() != null) entity.setLongitude(dto.getLongitude());
        if (dto.getLatitude() != null) entity.setLatitude(dto.getLatitude());
        if (dto.getLampType() != null) entity.setLampType(dto.getLampType());
        if (dto.getPower() != null) entity.setPower(dto.getPower());
        if (dto.getHeight() != null) entity.setHeight(dto.getHeight());
        if (dto.getAreaId() != null) entity.setAreaId(dto.getAreaId());
        if (dto.getCabinetId() != null) entity.setCabinetId(dto.getCabinetId());
        if (dto.getDeviceStatus() != null) entity.setDeviceStatus(dto.getDeviceStatus());
        streetlightRepository.save(entity);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        streetlightRepository.deleteById(id);
        return Result.success();
    }
}
