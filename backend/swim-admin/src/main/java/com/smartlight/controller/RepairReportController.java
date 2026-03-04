package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.dto.RepairReportDTO;
import com.smartlight.entity.RepairReport;
import com.smartlight.entity.User;
import com.smartlight.repository.RepairReportRepository;
import com.smartlight.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

@RestController
@RequestMapping("/api/repair-reports")
@RequiredArgsConstructor
public class RepairReportController {

    private final RepairReportRepository repairReportRepository;
    private final UserRepository userRepository;

    @GetMapping
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) String keyword) {
        Page<RepairReport> p = repairReportRepository.search(status, keyword,
                PageRequest.of(page - 1, size, Sort.by("createdAt").descending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/my")
    public Result<?> myReports(@RequestParam(defaultValue = "1") Integer page,
                               @RequestParam(defaultValue = "10") Integer size) {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        User user = userRepository.findByUsername(auth.getName()).orElseThrow();
        Page<RepairReport> p = repairReportRepository.findByReporterIdOrderByCreatedAtDesc(user.getId(),
                PageRequest.of(page - 1, size));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(repairReportRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("报修不存在")));
    }

    @PostMapping
    public Result<?> create(@RequestBody RepairReportDTO dto) {
        RepairReport report = new RepairReport();
        report.setReportNo("RP-" + LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyyMMdd-HHmmss")));
        report.setReporterName(dto.getReporterName());
        report.setReporterPhone(dto.getReporterPhone());
        report.setStreetlightId(dto.getStreetlightId());
        report.setAddress(dto.getAddress());
        report.setLongitude(dto.getLongitude());
        report.setLatitude(dto.getLatitude());
        report.setDescription(dto.getDescription());
        report.setImages(dto.getImages());
        report.setStatus(0);
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        if (auth != null && auth.isAuthenticated() && !"anonymousUser".equals(auth.getName())) {
            User user = userRepository.findByUsername(auth.getName()).orElse(null);
            if (user != null) {
                report.setReporterId(user.getId());
                if (report.getReporterName() == null) report.setReporterName(user.getRealName());
                if (report.getReporterPhone() == null) report.setReporterPhone(user.getPhone());
            }
        }
        repairReportRepository.save(report);
        return Result.success("报修提交成功", null);
    }

    @PutMapping("/{id}/handle")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> handle(@PathVariable Long id, @RequestBody java.util.Map<String, Object> body) {
        RepairReport report = repairReportRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("报修不存在"));
        if (body.containsKey("status")) report.setStatus((Integer) body.get("status"));
        if (body.containsKey("reply")) report.setReply((String) body.get("reply"));
        if (body.containsKey("workOrderId")) report.setWorkOrderId(Long.valueOf(body.get("workOrderId").toString()));
        repairReportRepository.save(report);
        return Result.success();
    }
}
