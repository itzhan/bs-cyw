-- ============================================
-- 城市智慧路灯管理信息系统 - 数据库初始化脚本
-- ============================================

CREATE DATABASE IF NOT EXISTS smart_streetlight DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE smart_streetlight;

SET NAMES utf8mb4;
SET CHARACTER_SET_CLIENT = utf8mb4;
SET CHARACTER_SET_RESULTS = utf8mb4;
SET CHARACTER_SET_CONNECTION = utf8mb4;

-- 先删除存在外健依赖的表
DROP TABLE IF EXISTS mqtt_message;
DROP TABLE IF EXISTS control_log;
DROP TABLE IF EXISTS energy_record;
DROP TABLE IF EXISTS work_order;
DROP TABLE IF EXISTS alarm;
DROP TABLE IF EXISTS repair_report;
DROP TABLE IF EXISTS streetlight;
DROP TABLE IF EXISTS control_strategy;
DROP TABLE IF EXISTS cabinet;
DROP TABLE IF EXISTS announcement;
DROP TABLE IF EXISTS user_role;
DROP TABLE IF EXISTS `user`;
DROP TABLE IF EXISTS role;
DROP TABLE IF EXISTS area;

-- ============================================
-- 1. 角色表
-- ============================================
CREATE TABLE role (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL UNIQUE COMMENT '角色名称: ADMIN/OPERATOR/USER',
    description VARCHAR(200) COMMENT '角色描述',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='角色表';

-- ============================================
-- 2. 用户表
-- ============================================
CREATE TABLE `user` (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) NOT NULL UNIQUE COMMENT '用户名',
    password VARCHAR(255) NOT NULL COMMENT '密码(BCrypt加密)',
    real_name VARCHAR(50) COMMENT '真实姓名',
    phone VARCHAR(20) COMMENT '手机号',
    email VARCHAR(100) COMMENT '邮箱',
    avatar VARCHAR(500) COMMENT '头像URL',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-禁用 1-启用',
    last_login_time DATETIME COMMENT '最后登录时间',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_username (username),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户表';

-- ============================================
-- 3. 用户-角色关联表
-- ============================================
CREATE TABLE user_role (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    user_id BIGINT NOT NULL,
    role_id BIGINT NOT NULL,
    UNIQUE INDEX idx_user_role (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES `user`(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户-角色关联表';

-- ============================================
-- 4. 区域表
-- ============================================
CREATE TABLE area (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL COMMENT '区域名称',
    code VARCHAR(50) NOT NULL UNIQUE COMMENT '区域编码',
    parent_id BIGINT DEFAULT 0 COMMENT '父级区域ID, 0为顶级',
    level TINYINT NOT NULL DEFAULT 1 COMMENT '层级: 1-区 2-街道 3-路段',
    description VARCHAR(500) COMMENT '描述',
    sort_order INT DEFAULT 0 COMMENT '排序',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-禁用 1-启用',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_parent_id (parent_id),
    INDEX idx_code (code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='区域表';

-- ============================================
-- 5. 电柜表
-- ============================================
CREATE TABLE cabinet (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(50) NOT NULL UNIQUE COMMENT '电柜编码',
    name VARCHAR(100) NOT NULL COMMENT '电柜名称',
    area_id BIGINT NOT NULL COMMENT '所属区域ID',
    address VARCHAR(500) COMMENT '安装地址',
    longitude DECIMAL(10, 7) COMMENT '经度',
    latitude DECIMAL(10, 7) COMMENT '纬度',
    capacity INT COMMENT '容量(路灯数)',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-故障 1-正常 2-维护中',
    install_date DATE COMMENT '安装日期',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_area_id (area_id),
    INDEX idx_status (status),
    FOREIGN KEY (area_id) REFERENCES area(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='电柜表';

-- ============================================
-- 6. 路灯设备表
-- ============================================
CREATE TABLE streetlight (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(50) NOT NULL UNIQUE COMMENT '路灯编码',
    device_uid VARCHAR(100) COMMENT '设备唯一标识(MQTT设备ID)',
    name VARCHAR(100) NOT NULL COMMENT '路灯名称',
    area_id BIGINT NOT NULL COMMENT '所属区域ID',
    cabinet_id BIGINT COMMENT '所属电柜ID',
    address VARCHAR(500) COMMENT '安装详细地址',
    longitude DECIMAL(10, 7) COMMENT '经度(北斗定位)',
    latitude DECIMAL(10, 7) COMMENT '纬度(北斗定位)',
    lamp_type VARCHAR(50) COMMENT '灯型: LED/钠灯/太阳能',
    hardware_model VARCHAR(100) COMMENT '硬件型号',
    electrical_params VARCHAR(500) COMMENT '电气参数(JSON: 额定电压/电流/频率等)',
    protection_rating VARCHAR(20) COMMENT '防护等级(如IP65)',
    power INT COMMENT '额定功率(W)',
    height DECIMAL(5, 2) COMMENT '灯杆高度(m)',
    brightness TINYINT DEFAULT 100 COMMENT '当前亮度(0-100)',
    online_status TINYINT NOT NULL DEFAULT 1 COMMENT '在线状态: 0-离线 1-在线',
    light_status TINYINT NOT NULL DEFAULT 0 COMMENT '亮灯状态: 0-关 1-开',
    device_status TINYINT NOT NULL DEFAULT 1 COMMENT '设备状态: 0-故障 1-在线正常 2-在线异常 3-离线 4-待检修 5-暂停运行',
    install_date DATE COMMENT '安装日期',
    last_maintain_date DATE COMMENT '最近维护日期',
    voltage DECIMAL(6, 2) COMMENT '当前电压(V)',
    current_val DECIMAL(6, 3) COMMENT '当前电流(A)',
    temperature DECIMAL(5, 2) COMMENT '当前温度(℃)',
    running_hours INT DEFAULT 0 COMMENT '累计运行小时数',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_device_uid (device_uid),
    INDEX idx_area_id (area_id),
    INDEX idx_cabinet_id (cabinet_id),
    INDEX idx_online_status (online_status),
    INDEX idx_device_status (device_status),
    FOREIGN KEY (area_id) REFERENCES area(id),
    FOREIGN KEY (cabinet_id) REFERENCES cabinet(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='路灯设备表';

-- ============================================
-- 7. 控制策略表
-- ============================================
CREATE TABLE control_strategy (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL COMMENT '策略名称',
    group_no TINYINT COMMENT '策略分组编号(1-16)',
    type TINYINT NOT NULL COMMENT '策略类型: 1-定时策略 2-光控策略 3-节假日策略 4-人流策略',
    action_type TINYINT DEFAULT 1 COMMENT '动作类型: 1-时间点+动作 2-经纬度+动作',
    area_id BIGINT COMMENT '适用区域ID(NULL表示全局)',
    description VARCHAR(500) COMMENT '策略描述',
    start_time TIME COMMENT '开灯时间',
    end_time TIME COMMENT '关灯时间',
    brightness TINYINT COMMENT '亮度(0-100)',
    light_threshold INT COMMENT '光照阈值(lux)',
    target_longitude DECIMAL(10, 7) COMMENT '目标经度(经纬度+动作类型)',
    target_latitude DECIMAL(10, 7) COMMENT '目标纬度(经纬度+动作类型)',
    effective_start DATE COMMENT '生效开始日期',
    effective_end DATE COMMENT '生效结束日期',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-禁用 1-启用',
    priority INT DEFAULT 0 COMMENT '优先级(越大越高)',
    created_by BIGINT COMMENT '创建人ID',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_group_no (group_no),
    INDEX idx_type (type),
    INDEX idx_status (status),
    INDEX idx_area_id (area_id),
    FOREIGN KEY (area_id) REFERENCES area(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='控制策略表';

-- ============================================
-- 8. 告警事件表
-- ============================================
CREATE TABLE alarm (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    alarm_code VARCHAR(50) NOT NULL UNIQUE COMMENT '告警编码',
    type TINYINT NOT NULL COMMENT '告警类型: 1-灯具故障 2-电压异常 3-电流异常 4-通信故障 5-线缆异常 6-温度异常 7-漏电告警 8-其他',
    level TINYINT NOT NULL DEFAULT 2 COMMENT '告警级别: 1-低 2-中 3-高 4-紧急',
    streetlight_id BIGINT COMMENT '关联路灯ID',
    cabinet_id BIGINT COMMENT '关联电柜ID',
    area_id BIGINT COMMENT '所属区域ID',
    title VARCHAR(200) NOT NULL COMMENT '告警标题',
    description TEXT COMMENT '告警描述',
    status TINYINT NOT NULL DEFAULT 0 COMMENT '状态: 0-未处理 1-处理中 2-已处理 3-已忽略',
    handler_id BIGINT COMMENT '处理人ID',
    handle_time DATETIME COMMENT '处理时间',
    handle_remark VARCHAR(500) COMMENT '处理备注',
    alarm_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '告警时间',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_type (type),
    INDEX idx_level (level),
    INDEX idx_status (status),
    INDEX idx_streetlight_id (streetlight_id),
    INDEX idx_alarm_time (alarm_time),
    FOREIGN KEY (streetlight_id) REFERENCES streetlight(id) ON DELETE SET NULL,
    FOREIGN KEY (cabinet_id) REFERENCES cabinet(id) ON DELETE SET NULL,
    FOREIGN KEY (area_id) REFERENCES area(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='告警事件表';

-- ============================================
-- 9. 维修工单表
-- ============================================
CREATE TABLE work_order (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    order_no VARCHAR(50) NOT NULL UNIQUE COMMENT '工单编号',
    alarm_id BIGINT COMMENT '关联告警ID',
    streetlight_id BIGINT COMMENT '关联路灯ID',
    area_id BIGINT COMMENT '所属区域ID',
    title VARCHAR(200) NOT NULL COMMENT '工单标题',
    description TEXT COMMENT '问题描述',
    priority TINYINT NOT NULL DEFAULT 2 COMMENT '优先级: 1-低 2-中 3-高 4-紧急',
    status TINYINT NOT NULL DEFAULT 0 COMMENT '状态: 0-待分配 1-已分配 2-处理中 3-已完成 4-已关闭',
    assignee_id BIGINT COMMENT '指派处理人ID',
    reporter_id BIGINT COMMENT '上报人ID',
    expected_finish DATETIME COMMENT '预计完成时间',
    actual_finish DATETIME COMMENT '实际完成时间',
    repair_content TEXT COMMENT '维修内容',
    repair_cost DECIMAL(10, 2) COMMENT '维修费用',
    images VARCHAR(2000) COMMENT '图片URL(JSON数组)',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_order_no (order_no),
    INDEX idx_status (status),
    INDEX idx_assignee_id (assignee_id),
    INDEX idx_alarm_id (alarm_id),
    FOREIGN KEY (alarm_id) REFERENCES alarm(id) ON DELETE SET NULL,
    FOREIGN KEY (streetlight_id) REFERENCES streetlight(id) ON DELETE SET NULL,
    FOREIGN KEY (area_id) REFERENCES area(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='维修工单表';

-- ============================================
-- 10. 能耗记录表
-- ============================================
CREATE TABLE energy_record (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    streetlight_id BIGINT COMMENT '路灯ID',
    cabinet_id BIGINT COMMENT '电柜ID',
    area_id BIGINT COMMENT '区域ID',
    record_date DATE NOT NULL COMMENT '记录日期',
    energy_kwh DECIMAL(10, 3) NOT NULL DEFAULT 0 COMMENT '耗电量(kWh)',
    running_minutes INT NOT NULL DEFAULT 0 COMMENT '运行时长(分钟)',
    avg_power DECIMAL(8, 2) COMMENT '平均功率(W)',
    peak_power DECIMAL(8, 2) COMMENT '峰值功率(W)',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_streetlight_id (streetlight_id),
    INDEX idx_area_id (area_id),
    INDEX idx_record_date (record_date),
    FOREIGN KEY (streetlight_id) REFERENCES streetlight(id) ON DELETE SET NULL,
    FOREIGN KEY (cabinet_id) REFERENCES cabinet(id) ON DELETE SET NULL,
    FOREIGN KEY (area_id) REFERENCES area(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='能耗记录表';

-- ============================================
-- 11. 控制操作日志表
-- ============================================
CREATE TABLE control_log (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    streetlight_id BIGINT COMMENT '路灯ID',
    area_id BIGINT COMMENT '区域ID(批量控制时)',
    action VARCHAR(50) NOT NULL COMMENT '操作: TURN_ON/TURN_OFF/SET_BRIGHTNESS/APPLY_STRATEGY',
    detail VARCHAR(500) COMMENT '操作详情',
    operator_id BIGINT COMMENT '操作人ID',
    strategy_id BIGINT COMMENT '关联策略ID(策略触发时)',
    result TINYINT NOT NULL DEFAULT 1 COMMENT '结果: 0-失败 1-成功',
    remark VARCHAR(500) COMMENT '备注',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_streetlight_id (streetlight_id),
    INDEX idx_operator_id (operator_id),
    INDEX idx_created_at (created_at),
    FOREIGN KEY (streetlight_id) REFERENCES streetlight(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='控制操作日志表';

-- ============================================
-- 12. 报修申请表(市民端)
-- ============================================
CREATE TABLE repair_report (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    report_no VARCHAR(50) NOT NULL UNIQUE COMMENT '报修编号',
    reporter_id BIGINT COMMENT '报修人ID',
    reporter_name VARCHAR(50) COMMENT '报修人姓名(非注册用户)',
    reporter_phone VARCHAR(20) COMMENT '联系电话',
    streetlight_id BIGINT COMMENT '关联路灯ID',
    address VARCHAR(500) COMMENT '故障地址',
    longitude DECIMAL(10, 7) COMMENT '经度',
    latitude DECIMAL(10, 7) COMMENT '纬度',
    description TEXT COMMENT '问题描述',
    images VARCHAR(2000) COMMENT '图片URL(JSON数组)',
    status TINYINT NOT NULL DEFAULT 0 COMMENT '状态: 0-待审核 1-已受理 2-处理中 3-已完成 4-已驳回',
    work_order_id BIGINT COMMENT '关联工单ID',
    reply VARCHAR(500) COMMENT '回复内容',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_report_no (report_no),
    INDEX idx_status (status),
    INDEX idx_reporter_id (reporter_id),
    FOREIGN KEY (streetlight_id) REFERENCES streetlight(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='报修申请表';

-- ============================================
-- 13. 系统公告表
-- ============================================
CREATE TABLE announcement (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(200) NOT NULL COMMENT '公告标题',
    content TEXT COMMENT '公告内容',
    type TINYINT NOT NULL DEFAULT 1 COMMENT '类型: 1-系统通知 2-维护公告 3-政策通知',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-草稿 1-已发布 2-已撤回',
    top_flag TINYINT NOT NULL DEFAULT 0 COMMENT '是否置顶: 0-否 1-是',
    publisher_id BIGINT COMMENT '发布人ID',
    publish_time DATETIME COMMENT '发布时间',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (status),
    INDEX idx_type (type),
    INDEX idx_publish_time (publish_time)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='系统公告表';

-- ============================================
-- 14. MQTT通信消息表
-- ============================================
CREATE TABLE mqtt_message (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    device_uid VARCHAR(100) COMMENT '设备唯一标识',
    topic VARCHAR(200) NOT NULL COMMENT 'MQTT主题',
    payload TEXT COMMENT '消息内容(JSON)',
    direction TINYINT NOT NULL DEFAULT 1 COMMENT '方向: 1-上行(设备→平台) 2-下行(平台→设备)',
    status TINYINT NOT NULL DEFAULT 1 COMMENT '状态: 0-失败 1-成功',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_device_uid (device_uid),
    INDEX idx_topic (topic),
    INDEX idx_direction (direction),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='MQTT通信消息表';
