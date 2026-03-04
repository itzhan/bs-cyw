package com.smartlight.controller;

import com.smartlight.common.Result;
import com.smartlight.dto.ControlDTO;
import com.smartlight.entity.ControlLog;
import com.smartlight.entity.Streetlight;
import com.smartlight.entity.User;
import com.smartlight.repository.ControlLogRepository;
import com.smartlight.repository.StreetlightRepository;
import com.smartlight.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/control")
@RequiredArgsConstructor
public class ControlController {

    private final StreetlightRepository streetlightRepository;
    private final ControlLogRepository controlLogRepository;
    private final UserRepository userRepository;

    @PostMapping("/execute")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> execute(@RequestBody ControlDTO dto) {
        String username = SecurityContextHolder.getContext().getAuthentication().getName();
        User operator = userRepository.findByUsername(username).orElse(null);
        Long operatorId = operator != null ? operator.getId() : null;
        List<Streetlight> lights;
        if (dto.getStreetlightIds() != null && !dto.getStreetlightIds().isEmpty()) {
            lights = streetlightRepository.findAllById(dto.getStreetlightIds());
        } else if (dto.getAreaId() != null) {
            lights = streetlightRepository.findByAreaId(dto.getAreaId());
        } else {
            return Result.error(400, "请指定路灯或区域");
        }
        int successCount = 0;
        for (Streetlight light : lights) {
            if (light.getOnlineStatus() == 0) continue;
            boolean success = true;
            switch (dto.getAction()) {
                case "TURN_ON":
                    light.setLightStatus(1);
                    light.setBrightness(dto.getBrightness() != null ? dto.getBrightness() : 100);
                    break;
                case "TURN_OFF":
                    light.setLightStatus(0);
                    light.setBrightness(0);
                    break;
                case "SET_BRIGHTNESS":
                    if (dto.getBrightness() != null) {
                        light.setBrightness(dto.getBrightness());
                        light.setLightStatus(dto.getBrightness() > 0 ? 1 : 0);
                    }
                    break;
                default:
                    success = false;
            }
            if (success) {
                streetlightRepository.save(light);
                successCount++;
            }
            ControlLog log = new ControlLog();
            log.setStreetlightId(light.getId());
            log.setAreaId(light.getAreaId());
            log.setAction(dto.getAction());
            log.setDetail(dto.getAction() + " 亮度" + (dto.getBrightness() != null ? dto.getBrightness() + "%" : ""));
            log.setOperatorId(operatorId);
            log.setResult(success ? 1 : 0);
            log.setRemark(dto.getRemark());
            controlLogRepository.save(log);
        }
        return Result.success("成功控制 " + successCount + " 盏路灯", null);
    }

    @GetMapping("/logs")
    @PreAuthorize("hasAnyRole('ADMIN','OPERATOR')")
    public Result<?> logs(@RequestParam(defaultValue = "1") Integer page,
                          @RequestParam(defaultValue = "20") Integer size) {
        return Result.success(controlLogRepository.findAllByOrderByCreatedAtDesc(
                PageRequest.of(page - 1, size)));
    }
}
