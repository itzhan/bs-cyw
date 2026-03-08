# 城市智慧路灯 - 嵌入式设备模拟器

## 概述

本程序使用 **C 语言** 模拟路灯嵌入式控制器（MCU），通过 **MQTT 协议** 与管理平台后端通信。

## 系统架构

```
┌──────────────────────┐          ┌─────────────────────────┐
│   嵌入式设备 (C语言)   │  MQTT    │   管理平台 (Java/Spring) │
│                      │ ◄──────► │                         │
│  • 传感器数据采集      │          │  • 接收设备状态          │
│  • ADC 电压/电流读取   │          │  • 下发控制指令          │
│  • 温度/漏电监测       │          │  • 告警分析处理          │
│  • 北斗定位获取       │          │  • 能耗统计分析          │
│  • 控制指令执行       │          │  • 数据持久化           │
└──────────────────────┘          └─────────────────────────┘
         │                                    │
         │    ┌──────────────────┐             │
         └───►│  MQTT Broker     │◄────────────┘
              │  (Mosquitto)     │
              └──────────────────┘
```

## 通信协议

### 上行（设备→平台）

| Topic | 说明 | 频率 |
|:---|:---|:---|
| `streetlight/{uid}/status` | 设备状态上报 | 每5秒 |
| `streetlight/{uid}/alarm` | 异常告警上报 | 触发时 |

### 下行（平台→设备）

| Topic | 说明 |
|:---|:---|
| `streetlight/{uid}/control` | 控制指令下发 |

### 控制指令格式

```json
// 开灯
{"action": "TURN_ON", "brightness": 80}

// 关灯
{"action": "TURN_OFF"}

// 调光
{"action": "SET_BRIGHTNESS", "brightness": 50}
```

## 快速开始

### 1. 安装依赖

```bash
cd device-simulator
make install-deps
```

### 2. 启动 MQTT Broker

```bash
# macOS
brew services start mosquitto

# 或直接运行
mosquitto -d -p 1883
```

### 3. 编译 & 运行

```bash
# 编译
make

# 运行单个设备
make run

# 或指定设备UID
./streetlight_device tcp://localhost:1883 DEV-HYL-001

# 运行多个设备(后台)
make run-multi

# 停止所有设备
make stop
```

### 4. 测试通信

```bash
# 终端1: 监听设备上报
mosquitto_sub -h localhost -t "streetlight/+/status" -v

# 终端2: 发送控制指令
mosquitto_pub -h localhost -t "streetlight/DEV-HYL-001/control" \
  -m '{"action":"SET_BRIGHTNESS","brightness":50}'
```

## 功能特性

- **传感器模拟**: 电压、电流、温度随机波动，模拟真实 ADC 采集
- **8种异常检测**: 温度过高(>50℃)、漏电(>30mA)、电压异常(±10%)等
- **遗嘱消息(LWT)**: 设备异常断连时自动通知平台
- **控制响应**: 开灯/关灯/亮度调节指令实时执行
- **优雅退出**: Ctrl+C 发送离线状态后关闭
