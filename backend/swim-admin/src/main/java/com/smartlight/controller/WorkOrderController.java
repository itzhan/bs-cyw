package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.dto.WorkOrderDTO;
import com.smartlight.entity.WorkOrder;
import com.smartlight.repository.WorkOrderRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

@RestController
@RequestMapping("/api/work-orders")
@RequiredArgsConstructor
public class WorkOrderController {

    private final WorkOrderRepository workOrderRepository;

    @GetMapping
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) Long areaId,
                          @RequestParam(required = false) String keyword) {
        Page<WorkOrder> p = workOrderRepository.search(status, areaId, keyword,
                PageRequest.of(page - 1, size, Sort.by("createdAt").descending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(workOrderRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("工单不存在")));
    }

    @PostMapping
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> create(@RequestBody WorkOrderDTO dto) {
        WorkOrder wo = new WorkOrder();
        wo.setOrderNo("WO-" + LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyyMMdd-HHmmss")));
        wo.setAlarmId(dto.getAlarmId());
        wo.setStreetlightId(dto.getStreetlightId());
        wo.setAreaId(dto.getAreaId());
        wo.setTitle(dto.getTitle());
        wo.setDescription(dto.getDescription());
        wo.setPriority(dto.getPriority() != null ? dto.getPriority() : 2);
        wo.setAssigneeId(dto.getAssigneeId());
        wo.setExpectedFinish(dto.getExpectedFinish());
        wo.setStatus(dto.getAssigneeId() != null ? 1 : 0);
        workOrderRepository.save(wo);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> update(@PathVariable Long id, @RequestBody WorkOrderDTO dto) {
        WorkOrder wo = workOrderRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("工单不存在"));
        if (dto.getTitle() != null) wo.setTitle(dto.getTitle());
        if (dto.getDescription() != null) wo.setDescription(dto.getDescription());
        if (dto.getPriority() != null) wo.setPriority(dto.getPriority());
        if (dto.getAssigneeId() != null) { wo.setAssigneeId(dto.getAssigneeId()); if (wo.getStatus() == 0) wo.setStatus(1); }
        if (dto.getExpectedFinish() != null) wo.setExpectedFinish(dto.getExpectedFinish());
        if (dto.getRepairContent() != null) wo.setRepairContent(dto.getRepairContent());
        if (dto.getRepairCost() != null) wo.setRepairCost(dto.getRepairCost());
        workOrderRepository.save(wo);
        return Result.success();
    }

    @PutMapping("/{id}/status")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> updateStatus(@PathVariable Long id, @RequestBody java.util.Map<String, Object> body) {
        WorkOrder wo = workOrderRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("工单不存在"));
        Integer status = (Integer) body.get("status");
        wo.setStatus(status);
        if (status == 3) wo.setActualFinish(LocalDateTime.now());
        if (body.containsKey("repairContent")) wo.setRepairContent((String) body.get("repairContent"));
        workOrderRepository.save(wo);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        workOrderRepository.deleteById(id);
        return Result.success();
    }
}
