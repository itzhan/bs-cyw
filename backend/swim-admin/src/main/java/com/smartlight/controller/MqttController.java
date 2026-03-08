package com.smartlight.controller;

import com.smartlight.common.PageResult;
import com.smartlight.common.Result;
import com.smartlight.entity.MqttMessage;
import com.smartlight.repository.MqttMessageRepository;
import com.smartlight.service.MqttPublishService;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.LinkedHashMap;
import java.util.Map;

@RestController
@RequestMapping("/api/mqtt")
@RequiredArgsConstructor
public class MqttController {

    private final MqttMessageRepository mqttMessageRepository;
    private final MqttPublishService mqttPublishService;

    @GetMapping("/status")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> status() {
        Map<String, Object> data = new LinkedHashMap<>();
        data.put("connected", mqttPublishService.isConnected());
        data.put("broker", mqttPublishService.getBrokerInfo());
        data.put("totalUplink", mqttMessageRepository.countByDirection(1));
        data.put("totalDownlink", mqttMessageRepository.countByDirection(2));
        return Result.success(data);
    }

    @GetMapping("/messages")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> messages(@RequestParam(defaultValue = "1") Integer page,
                              @RequestParam(defaultValue = "20") Integer size,
                              @RequestParam(required = false) String deviceUid,
                              @RequestParam(required = false) Integer direction) {
        Page<MqttMessage> p = mqttMessageRepository.search(deviceUid, direction,
                PageRequest.of(page - 1, size));
        return Result.success(new PageResult<>(p.getContent(), p.getTotalElements(), page, size));
    }

    @PostMapping("/publish")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> publish(@RequestBody Map<String, String> body) {
        String deviceUid = body.get("deviceUid");
        String action = body.get("action");
        String payload = body.get("payload");
        if (deviceUid == null || payload == null) {
            return Result.error(400, "deviceUid 和 payload 不能为空");
        }
        boolean success = mqttPublishService.sendCommand(deviceUid, action, payload);
        return success ? Result.success("指令下发成功", null)
                       : Result.success("指令已记录，但 MQTT 未连接", null);
    }
}
