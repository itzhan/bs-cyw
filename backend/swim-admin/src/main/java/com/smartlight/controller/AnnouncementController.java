package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.entity.Announcement;
import com.smartlight.repository.AnnouncementRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;

@RestController
@RequestMapping("/api/announcements")
@RequiredArgsConstructor
public class AnnouncementController {

    private final AnnouncementRepository announcementRepository;

    @GetMapping("/published")
    public Result<?> published() {
        return Result.success(announcementRepository.findPublished());
    }

    @GetMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> list(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "10") Integer size,
                          @RequestParam(required = false) Integer status,
                          @RequestParam(required = false) Integer type) {
        Page<Announcement> p = announcementRepository.search(status, type,
                PageRequest.of(page - 1, size, Sort.by("createdAt").descending()));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @GetMapping("/{id}")
    public Result<?> detail(@PathVariable Long id) {
        return Result.success(announcementRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("公告不存在")));
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> create(@RequestBody Announcement announcement) {
        if (announcement.getStatus() == 1) announcement.setPublishTime(LocalDateTime.now());
        announcementRepository.save(announcement);
        return Result.success();
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> update(@PathVariable Long id, @RequestBody Announcement dto) {
        Announcement entity = announcementRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("公告不存在"));
        if (dto.getTitle() != null) entity.setTitle(dto.getTitle());
        if (dto.getContent() != null) entity.setContent(dto.getContent());
        if (dto.getType() != null) entity.setType(dto.getType());
        if (dto.getTopFlag() != null) entity.setTopFlag(dto.getTopFlag());
        announcementRepository.save(entity);
        return Result.success();
    }

    @PutMapping("/{id}/publish")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> publish(@PathVariable Long id) {
        Announcement entity = announcementRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("公告不存在"));
        entity.setStatus(1);
        entity.setPublishTime(LocalDateTime.now());
        announcementRepository.save(entity);
        return Result.success();
    }

    @PutMapping("/{id}/withdraw")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> withdraw(@PathVariable Long id) {
        Announcement entity = announcementRepository.findById(id)
                .orElseThrow(() -> new jakarta.persistence.EntityNotFoundException("公告不存在"));
        entity.setStatus(2);
        announcementRepository.save(entity);
        return Result.success();
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public Result<?> delete(@PathVariable Long id) {
        announcementRepository.deleteById(id);
        return Result.success();
    }
}
