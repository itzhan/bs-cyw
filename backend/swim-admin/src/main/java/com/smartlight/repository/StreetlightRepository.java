package com.smartlight.repository;

import com.smartlight.entity.Streetlight;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.util.List;

public interface StreetlightRepository extends JpaRepository<Streetlight, Long> {
    List<Streetlight> findByAreaId(Long areaId);
    List<Streetlight> findByCabinetId(Long cabinetId);
    long countByOnlineStatus(Integer onlineStatus);
    long countByLightStatus(Integer lightStatus);
    long countByDeviceStatus(Integer deviceStatus);
    @Query("SELECT s FROM Streetlight s WHERE (:areaId IS NULL OR s.areaId = :areaId) AND (:cabinetId IS NULL OR s.cabinetId = :cabinetId) AND (:onlineStatus IS NULL OR s.onlineStatus = :onlineStatus) AND (:deviceStatus IS NULL OR s.deviceStatus = :deviceStatus) AND (:keyword IS NULL OR s.name LIKE %:keyword% OR s.code LIKE %:keyword%)")
    Page<Streetlight> search(Long areaId, Long cabinetId, Integer onlineStatus, Integer deviceStatus, String keyword, Pageable pageable);
    @Query("SELECT s.lampType, COUNT(s) FROM Streetlight s GROUP BY s.lampType")
    List<Object[]> countByLampType();
    @Query("SELECT s.areaId, COUNT(s) FROM Streetlight s GROUP BY s.areaId")
    List<Object[]> countByArea();
}
