package com.smartlight.repository;

import com.smartlight.entity.ControlStrategy;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.util.List;

public interface ControlStrategyRepository extends JpaRepository<ControlStrategy, Long> {
    List<ControlStrategy> findByStatus(Integer status);
    @Query("SELECT c FROM ControlStrategy c WHERE (:type IS NULL OR c.type = :type) AND (:status IS NULL OR c.status = :status) AND (:keyword IS NULL OR c.name LIKE %:keyword%)")
    Page<ControlStrategy> search(Integer type, Integer status, String keyword, Pageable pageable);
}
