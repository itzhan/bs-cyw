package com.smartlight.repository;

import com.smartlight.entity.MqttMessage;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

public interface MqttMessageRepository extends JpaRepository<MqttMessage, Long> {

    Page<MqttMessage> findAllByOrderByCreatedAtDesc(Pageable pageable);

    @Query("SELECT m FROM MqttMessage m WHERE " +
            "(:deviceUid IS NULL OR m.deviceUid = :deviceUid) AND " +
            "(:direction IS NULL OR m.direction = :direction) " +
            "ORDER BY m.createdAt DESC")
    Page<MqttMessage> search(String deviceUid, Integer direction, Pageable pageable);

    long countByDirection(Integer direction);
}
