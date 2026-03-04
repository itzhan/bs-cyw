package com.smartlight.repository;

import com.smartlight.entity.Area;
import org.springframework.data.jpa.repository.JpaRepository;
import java.util.List;

public interface AreaRepository extends JpaRepository<Area, Long> {
    List<Area> findByParentIdOrderBySortOrder(Long parentId);
    List<Area> findByLevel(Integer level);
    List<Area> findByStatus(Integer status);
    boolean existsByCode(String code);
}
