package com.smartlight.repository;

import com.smartlight.entity.Cabinet;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.util.List;

public interface CabinetRepository extends JpaRepository<Cabinet, Long> {
    List<Cabinet> findByAreaId(Long areaId);
    @Query("SELECT c FROM Cabinet c WHERE (:areaId IS NULL OR c.areaId = :areaId) AND (:status IS NULL OR c.status = :status) AND (:keyword IS NULL OR c.name LIKE %:keyword% OR c.code LIKE %:keyword%)")
    Page<Cabinet> search(Long areaId, Integer status, String keyword, Pageable pageable);
    long countByStatus(Integer status);
}
