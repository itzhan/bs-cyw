package com.smartlight.controller;

import com.smartlight.common.Result;
import com.smartlight.repository.*;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;

@RestController
@RequestMapping("/api/statistics")
@RequiredArgsConstructor
public class StatisticsController {

    private final StreetlightRepository streetlightRepository;
    private final AlarmRepository alarmRepository;
    private final WorkOrderRepository workOrderRepository;
    private final EnergyRecordRepository energyRecordRepository;
    private final CabinetRepository cabinetRepository;
    private final RepairReportRepository repairReportRepository;

    @GetMapping("/overview")
    public Result<?> overview() {
        Map<String, Object> data = new LinkedHashMap<>();
        long total = streetlightRepository.count();
        long online = streetlightRepository.countByOnlineStatus(1);
        long lightOn = streetlightRepository.countByLightStatus(1);
        long fault = streetlightRepository.countByDeviceStatus(0);
        data.put("totalLights", total);
        data.put("onlineCount", online);
        data.put("onlineRate", total > 0 ? Math.round(online * 10000.0 / total) / 100.0 : 0);
        data.put("lightOnCount", lightOn);
        data.put("lightOnRate", total > 0 ? Math.round(lightOn * 10000.0 / total) / 100.0 : 0);
        data.put("faultCount", fault);
        data.put("totalCabinets", cabinetRepository.count());
        long unhandledAlarms = alarmRepository.countByStatus(0);
        long todayAlarms = alarmRepository.countByAlarmTimeAfter(LocalDate.now().atStartOfDay());
        data.put("unhandledAlarms", unhandledAlarms);
        data.put("todayAlarms", todayAlarms);
        long pendingOrders = workOrderRepository.countByStatus(0) + workOrderRepository.countByStatus(1);
        data.put("pendingOrders", pendingOrders);
        long pendingReports = repairReportRepository.countByStatus(0);
        data.put("pendingReports", pendingReports);
        LocalDate today = LocalDate.now();
        BigDecimal todayEnergy = energyRecordRepository.totalEnergy(today, today);
        BigDecimal monthEnergy = energyRecordRepository.totalEnergy(today.withDayOfMonth(1), today);
        data.put("todayEnergy", todayEnergy != null ? todayEnergy : BigDecimal.ZERO);
        data.put("monthEnergy", monthEnergy != null ? monthEnergy : BigDecimal.ZERO);
        return Result.success(data);
    }

    @GetMapping("/energy/daily")
    public Result<?> energyDaily(@RequestParam(required = false) String startDate,
                                 @RequestParam(required = false) String endDate) {
        LocalDate start = startDate != null ? LocalDate.parse(startDate) : LocalDate.now().minusDays(30);
        LocalDate end = endDate != null ? LocalDate.parse(endDate) : LocalDate.now();
        List<Object[]> results = energyRecordRepository.dailyEnergy(start, end);
        List<Map<String, Object>> data = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> m = new HashMap<>();
            m.put("date", row[0].toString());
            m.put("energy", row[1]);
            data.add(m);
        }
        return Result.success(data);
    }

    @GetMapping("/energy/by-area")
    public Result<?> energyByArea(@RequestParam(required = false) String startDate,
                                  @RequestParam(required = false) String endDate) {
        LocalDate start = startDate != null ? LocalDate.parse(startDate) : LocalDate.now().minusDays(30);
        LocalDate end = endDate != null ? LocalDate.parse(endDate) : LocalDate.now();
        return Result.success(energyRecordRepository.energyByArea(start, end));
    }

    @GetMapping("/alarm/by-type")
    public Result<?> alarmByType() {
        LocalDateTime thirtyDaysAgo = LocalDateTime.now().minusDays(30);
        List<Object[]> results = alarmRepository.countByTypeSince(thirtyDaysAgo);
        Map<String, String> typeNames = Map.of("1", "灯具故障", "2", "电压异常", "3", "电流异常", "4", "通信故障", "5", "线缆异常", "6", "温度异常", "7", "漏电告警", "8", "其他");
        List<Map<String, Object>> data = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> m = new HashMap<>();
            m.put("type", row[0]);
            m.put("typeName", typeNames.getOrDefault(row[0].toString(), "其他"));
            m.put("count", row[1]);
            data.add(m);
        }
        return Result.success(data);
    }

    @GetMapping("/alarm/by-level")
    public Result<?> alarmByLevel() {
        LocalDateTime thirtyDaysAgo = LocalDateTime.now().minusDays(30);
        List<Object[]> results = alarmRepository.countByLevelSince(thirtyDaysAgo);
        Map<String, String> levelNames = Map.of("1", "低", "2", "中", "3", "高", "4", "紧急");
        List<Map<String, Object>> data = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> m = new HashMap<>();
            m.put("level", row[0]);
            m.put("levelName", levelNames.getOrDefault(row[0].toString(), "未知"));
            m.put("count", row[1]);
            data.add(m);
        }
        return Result.success(data);
    }

    @GetMapping("/device/by-type")
    public Result<?> deviceByType() {
        List<Object[]> results = streetlightRepository.countByLampType();
        List<Map<String, Object>> data = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> m = new HashMap<>();
            m.put("lampType", row[0]);
            m.put("count", row[1]);
            data.add(m);
        }
        return Result.success(data);
    }

    @GetMapping("/device/by-area")
    public Result<?> deviceByArea() {
        List<Object[]> results = streetlightRepository.countByArea();
        List<Map<String, Object>> data = new ArrayList<>();
        for (Object[] row : results) {
            Map<String, Object> m = new HashMap<>();
            m.put("areaId", row[0]);
            m.put("count", row[1]);
            data.add(m);
        }
        return Result.success(data);
    }
}
