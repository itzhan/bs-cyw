-- ============================================
-- v2 迁移(MySQL 8 兼容): 通过存储过程实现幂等列添加
-- 新库请先跑 init.sql(已含新列),此文件只给已有数据的库打补丁。
-- 执行: docker exec -i <mysql_container> mysql -uroot -pXXX smart_streetlight < migration_v2.sql
-- ============================================

USE smart_streetlight;

DROP PROCEDURE IF EXISTS add_col_if_missing;
DELIMITER //
CREATE PROCEDURE add_col_if_missing(IN tbl VARCHAR(64), IN col VARCHAR(64), IN ddl TEXT)
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
                 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = tbl AND COLUMN_NAME = col) THEN
    SET @sql = CONCAT('ALTER TABLE `', tbl, '` ADD COLUMN ', ddl);
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;
END//
DELIMITER ;

CALL add_col_if_missing('control_strategy', 'start_datetime', "start_datetime DATETIME COMMENT '精确开始时刻'");
CALL add_col_if_missing('control_strategy', 'end_datetime',   "end_datetime DATETIME COMMENT '精确结束时刻'");
CALL add_col_if_missing('control_strategy', 'last_phase',     "last_phase TINYINT DEFAULT 0 COMMENT '0未开始 1执行中 2已结束'");
CALL add_col_if_missing('work_order',       'repair_report_id', "repair_report_id BIGINT COMMENT '关联报修ID'");

DROP PROCEDURE add_col_if_missing;
