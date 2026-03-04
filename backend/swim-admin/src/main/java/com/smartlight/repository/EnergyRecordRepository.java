package com.smartlight.repository;

import com.smartlight.entity.EnergyRecord;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.time.LocalDate;
import java.util.List;

public interface EnergyRecordRepository extends JpaRepository<EnergyRecord, Long> {
    @Query("SELECT e.recordDate, SUM(e.energyKwh) FROM EnergyRecord e WHERE e.recordDate BETWEEN :start AND :end GROUP BY e.recordDate ORDER BY e.recordDate")
    List<Object[]> dailyEnergy(LocalDate start, LocalDate end);
    @Query("SELECT e.areaId, SUM(e.energyKwh) FROM EnergyRecord e WHERE e.recordDate BETWEEN :start AND :end GROUP BY e.areaId")
    List<Object[]> energyByArea(LocalDate start, LocalDate end);
    @Query("SELECT SUM(e.energyKwh) FROM EnergyRecord e WHERE e.recordDate BETWEEN :start AND :end")
    java.math.BigDecimal totalEnergy(LocalDate start, LocalDate end);
}
