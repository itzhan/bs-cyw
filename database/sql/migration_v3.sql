-- ============================================
-- v3 迁移: 新增系统设置表 system_setting(幂等)
-- 新库请先跑 init.sql(已含此表),此文件只给已有数据的库打补丁。
-- 执行: docker exec -i <mysql_container> mysql -uroot -pXXX smart_streetlight < migration_v3.sql
-- ============================================

USE smart_streetlight;

CREATE TABLE IF NOT EXISTS system_setting (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    `key` VARCHAR(100) NOT NULL UNIQUE COMMENT '配置键',
    `value` VARCHAR(500) NOT NULL COMMENT '配置值',
    description VARCHAR(500) COMMENT '配置描述',
    category VARCHAR(50) NOT NULL DEFAULT 'general' COMMENT '分类: energy/general/...',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='系统设置表';

-- 默认配置(若已存在则跳过)
INSERT IGNORE INTO system_setting (`key`, `value`, description, category) VALUES
('energy.kwh_per_minute', '0.6', '每分钟每盏亮灯消耗电量(kWh)，用于能耗累计', 'energy');
