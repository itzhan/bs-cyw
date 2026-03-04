package com.smartlight.repository;

import com.smartlight.entity.Alarm;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.time.LocalDateTime;
import java.util.List;

public interface AlarmRepository extends JpaRepository<Alarm, Long> {
    long countByStatus(Integer status);
    @Query("SELECT a FROM Alarm a WHERE (:type IS NULL OR a.type = :type) AND (:level IS NULL OR a.level = :level) AND (:status IS NULL OR a.status = :status) AND (:areaId IS NULL OR a.areaId = :areaId) AND (:keyword IS NULL OR a.title LIKE %:keyword%)")
    Page<Alarm> search(Integer type, Integer level, Integer status, Long areaId, String keyword, Pageable pageable);
    @Query("SELECT a.type, COUNT(a) FROM Alarm a WHERE a.alarmTime >= :start GROUP BY a.type")
    List<Object[]> countByTypeSince(LocalDateTime start);
    @Query("SELECT a.level, COUNT(a) FROM Alarm a WHERE a.alarmTime >= :start GROUP BY a.level")
    List<Object[]> countByLevelSince(LocalDateTime start);
    long countByAlarmTimeAfter(LocalDateTime time);
}
