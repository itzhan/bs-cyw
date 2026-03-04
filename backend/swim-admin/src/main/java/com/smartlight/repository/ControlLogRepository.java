package com.smartlight.repository;

import com.smartlight.entity.ControlLog;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ControlLogRepository extends JpaRepository<ControlLog, Long> {
    Page<ControlLog> findByStreetlightIdOrderByCreatedAtDesc(Long streetlightId, Pageable pageable);
    Page<ControlLog> findAllByOrderByCreatedAtDesc(Pageable pageable);
}
