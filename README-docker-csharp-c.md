# 智慧路灯（Frontend + C# Backend + C 设备模拟）Docker 一键部署

## 1. 启动

在项目根目录执行：

```bash
cd /Users/itzhan/Desktop/毕业设计/陈勇忘
DOCKER_BUILDKIT=0 docker compose up -d --build
```

## 2. 访问

- 前端地址：`http://localhost:3001/pages/login.html`
- 默认账号：`admin`
- 默认密码：`123456`

## 3. 当前编排服务

- `frontend`：前端 Nginx（对外 `3001`）
- `backend`：C# Web API（仅容器内开放 `8080`，由前端 `/api` 反代）
- `mysql`：MySQL 8（仅容器内使用）
- `mqtt`：Mosquitto（对外 `1884`）
- `sim-device-1/2/3`：C 语言设备模拟器

## 4. 停止

```bash
docker compose down
```

## 5. 验证要点

### 5.1 查看服务状态

```bash
docker compose ps
```

### 5.2 查看后端日志（确认 MQTT 已连接并订阅）

```bash
docker compose logs backend --tail 100
```

### 5.3 验证 C 设备在上报

```bash
docker compose exec mqtt sh -lc \
  'mosquitto_sub -h localhost -p 1883 -t "streetlight/+/status" -C 3 -v'
```

## 6. 功能说明（本编排已覆盖）

- 前端核心接口可用（登录、统计、设备、区域、电柜、告警、工单、策略、公告、用户、MQTT页）
- MQTT 下行：前端下发控制指令 -> 后端 -> C 设备
- MQTT 上行：C 设备状态/告警 -> 后端订阅入库（`mqtt_message`），并同步回写 `streetlight` 状态

## 7. 端口冲突说明

本编排默认使用：

- 前端：`3001`
- MQTT：`1884`

如果你机器端口冲突，可改 `docker-compose.yml` 中 `ports` 再重启。
