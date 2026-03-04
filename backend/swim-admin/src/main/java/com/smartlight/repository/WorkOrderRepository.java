package com.smartlight.repository;

import com.smartlight.entity.WorkOrder;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

public interface WorkOrderRepository extends JpaRepository<WorkOrder, Long> {
    long countByStatus(Integer status);
    @Query("SELECT w FROM WorkOrder w WHERE (:status IS NULL OR w.status = :status) AND (:areaId IS NULL OR w.areaId = :areaId) AND (:keyword IS NULL OR w.title LIKE %:keyword% OR w.orderNo LIKE %:keyword%)")
    Page<WorkOrder> search(Integer status, Long areaId, String keyword, Pageable pageable);
}
