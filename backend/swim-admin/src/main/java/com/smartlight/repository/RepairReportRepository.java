package com.smartlight.repository;

import com.smartlight.entity.RepairReport;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

public interface RepairReportRepository extends JpaRepository<RepairReport, Long> {
    Page<RepairReport> findByReporterIdOrderByCreatedAtDesc(Long reporterId, Pageable pageable);
    @Query("SELECT r FROM RepairReport r WHERE (:status IS NULL OR r.status = :status) AND (:keyword IS NULL OR r.reporterName LIKE %:keyword% OR r.reportNo LIKE %:keyword%)")
    Page<RepairReport> search(Integer status, String keyword, Pageable pageable);
    long countByStatus(Integer status);
}
