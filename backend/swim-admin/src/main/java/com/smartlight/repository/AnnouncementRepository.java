package com.smartlight.repository;

import com.smartlight.entity.Announcement;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import java.util.List;

public interface AnnouncementRepository extends JpaRepository<Announcement, Long> {
    @Query("SELECT a FROM Announcement a WHERE a.status = 1 ORDER BY a.topFlag DESC, a.publishTime DESC")
    List<Announcement> findPublished();
    @Query("SELECT a FROM Announcement a WHERE (:status IS NULL OR a.status = :status) AND (:type IS NULL OR a.type = :type)")
    Page<Announcement> search(Integer status, Integer type, Pageable pageable);
}
