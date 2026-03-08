/*
 * ============================================
 * 城市智慧路灯管理信息系统 - 嵌入式设备模拟器
 * ============================================
 *
 * 功能说明：
 *   本程序模拟路灯嵌入式控制器（MCU），通过 MQTT 协议与管理平台通信。
 *   支持以下功能：
 *   1. 周期性上报设备状态（电压、电流、温度、亮度、开关状态等）
 *   2. 订阅并响应平台下发的控制指令（开灯/关灯/亮度调节）
 *   3. 异常状态自动上报告警（温度过高、漏电检测等）
 *
 * 通信协议：
 *   - 上报 Topic: streetlight/{device_uid}/status
 *   - 控制 Topic: streetlight/{device_uid}/control
 *   - 数据格式: JSON
 *
 * 编译方法：
 *   gcc -o streetlight_device streetlight_device.c -lpaho-mqtt3c -lm -lpthread
 *   或使用 Makefile: make
 *
 * 运行方法：
 *   ./streetlight_device [broker_url] [device_uid]
 *   例如: ./streetlight_device tcp://localhost:1883 DEV-HYL-001
 *
 * 依赖库：
 *   Eclipse Paho MQTT C Client (libpaho-mqtt3c)
 *   安装: brew install eclipse-paho-mqtt-c  (macOS)
 *         apt-get install libpaho-mqtt-dev   (Ubuntu/Debian)
 *
 * 作者: 庄立博
 * 日期: 2026年3月
 */

#include <math.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h> /* sleep, usleep */

#include "MQTTClient.h"

/* ============================================
 * 常量定义
 * ============================================ */
#define DEFAULT_BROKER "tcp://localhost:1883"
#define DEFAULT_DEVICE_ID "DEV-HYL-001"
#define CLIENT_ID_PREFIX "SL-MCU-"
#define QOS 1
#define TIMEOUT 10000L
#define REPORT_INTERVAL 5 /* 状态上报间隔(秒) */
#define MAX_PAYLOAD_LEN 1024
#define MAX_TOPIC_LEN 256

/* 告警阈值 */
#define TEMP_WARN_THRESHOLD 50.0 /* 温度告警阈值(℃) */
#define TEMP_CRIT_THRESHOLD 60.0 /* 温度严重告警阈值(℃) */
#define LEAKAGE_THRESHOLD 30.0   /* 漏电电流阈值(mA) */
#define VOLTAGE_LOW 190.0        /* 电压偏低阈值(V) */
#define VOLTAGE_HIGH 250.0       /* 电压偏高阈值(V) */

/* ============================================
 * 路灯设备状态结构体
 * 模拟嵌入式MCU中的设备运行参数
 * ============================================ */
typedef struct {
  char device_uid[64];        /* 设备唯一标识 */
  char hardware_model[64];    /* 硬件型号 */
  char protection_rating[16]; /* 防护等级 */

  /* 电气参数 */
  double voltage;         /* 当前电压(V) */
  double current;         /* 当前电流(A) */
  double temperature;     /* 灯具温度(℃) */
  double leakage_current; /* 漏电电流(mA) */
  int power;              /* 额定功率(W) */
  double rated_voltage;   /* 额定电压(V) */
  double rated_current;   /* 额定电流(A) */
  int frequency;          /* 额定频率(Hz) */

  /* 运行状态 */
  int light_status;  /* 亮灯状态: 0-关 1-开 */
  int online_status; /* 在线状态: 0-离线 1-在线 */
  int device_status; /* 设备状态: 0-故障 1-正常 2-异常 3-离线 4-待检修 5-暂停 */
  int brightness;    /* 当前亮度(0-100) */

  /* 定位信息(北斗) */
  double longitude; /* 经度 */
  double latitude;  /* 纬度 */

  /* 运行统计 */
  unsigned long running_seconds; /* 累计运行秒数 */
  unsigned long total_reports;   /* 累计上报次数 */
} StreetlightDevice;

/* ============================================
 * 全局变量
 * ============================================ */
static volatile int g_running = 1; /* 运行标志 */
static MQTTClient g_client;        /* MQTT 客户端句柄 */
static StreetlightDevice g_device; /* 设备实例 */

/* ============================================
 * 信号处理 - 支持 Ctrl+C 优雅退出
 * ============================================ */
void signal_handler(int sig) {
  printf("\n[MCU] 收到信号 %d，设备即将关闭...\n", sig);
  g_running = 0;
}

/* ============================================
 * 工具函数: 生成随机浮点数波动
 * 模拟传感器读数的自然波动
 * ============================================ */
double random_fluctuation(double base, double range) {
  return base + ((double)rand() / RAND_MAX * 2.0 - 1.0) * range;
}

/* ============================================
 * 初始化设备参数
 * 模拟 MCU 上电自检过程
 * ============================================ */
void device_init(StreetlightDevice *dev, const char *device_uid) {
  memset(dev, 0, sizeof(StreetlightDevice));

  strncpy(dev->device_uid, device_uid, sizeof(dev->device_uid) - 1);
  strncpy(dev->hardware_model, "ZM-LED150A", sizeof(dev->hardware_model) - 1);
  strncpy(dev->protection_rating, "IP65", sizeof(dev->protection_rating) - 1);

  /* 电气额定参数 */
  dev->power = 150;
  dev->rated_voltage = 220.0;
  dev->rated_current = 0.68;
  dev->frequency = 50;

  /* 初始运行参数 */
  dev->voltage = 220.0;
  dev->current = 0.68;
  dev->temperature = 25.0;
  dev->leakage_current = 0.5;

  /* 默认状态: 在线、开灯、正常 */
  dev->online_status = 1;
  dev->light_status = 1;
  dev->device_status = 1;
  dev->brightness = 80;

  /* 北斗定位坐标(郑州花园路) */
  dev->longitude = 113.6720;
  dev->latitude = 34.7935;

  dev->running_seconds = 0;
  dev->total_reports = 0;

  printf("[MCU] 设备初始化完成\n");
  printf("      设备UID: %s\n", dev->device_uid);
  printf("      硬件型号: %s\n", dev->hardware_model);
  printf("      额定功率: %dW\n", dev->power);
  printf("      防护等级: %s\n", dev->protection_rating);
  printf("      北斗定位: %.4f, %.4f\n", dev->longitude, dev->latitude);
}

/* ============================================
 * 更新传感器读数
 * 模拟 ADC 采集过程，加入自然波动
 * ============================================ */
void device_update_sensors(StreetlightDevice *dev) {
  if (dev->light_status == 1) {
    /* 灯亮时的正常读数波动 */
    dev->voltage = random_fluctuation(dev->rated_voltage, 3.0);
    dev->current = random_fluctuation(dev->rated_current, 0.02);

    /* 温度随运行时间缓慢上升到稳定值 */
    double target_temp = 35.0 + (dev->brightness / 100.0) * 15.0;
    dev->temperature += (target_temp - dev->temperature) * 0.1;
    dev->temperature = random_fluctuation(dev->temperature, 0.5);

    /* 漏电电流微小波动 */
    dev->leakage_current = random_fluctuation(0.5, 0.3);
    if (dev->leakage_current < 0)
      dev->leakage_current = 0.1;

    dev->running_seconds += REPORT_INTERVAL;
  } else {
    /* 灯关时 */
    dev->voltage = random_fluctuation(dev->rated_voltage, 1.0);
    dev->current = 0.0;
    dev->temperature += (25.0 - dev->temperature) * 0.15; /* 降温 */
    dev->leakage_current = random_fluctuation(0.2, 0.1);
    if (dev->leakage_current < 0)
      dev->leakage_current = 0;
  }
}

/* ============================================
 * 检查告警条件
 * 模拟 MCU 中的异常检测逻辑
 * ============================================ */
int device_check_alarms(StreetlightDevice *dev, char *alarm_msg, int msg_len) {
  alarm_msg[0] = '\0';

  /* 温度过高告警 */
  if (dev->temperature > TEMP_CRIT_THRESHOLD) {
    snprintf(alarm_msg, msg_len, "TEMP_CRITICAL: 温度%.1f℃超过严重阈值%.0f℃",
             dev->temperature, TEMP_CRIT_THRESHOLD);
    dev->device_status = 0; /* 故障 */
    return 6;               /* 告警类型6: 温度异常 */
  }
  if (dev->temperature > TEMP_WARN_THRESHOLD) {
    snprintf(alarm_msg, msg_len, "TEMP_HIGH: 温度%.1f℃超过警告阈值%.0f℃",
             dev->temperature, TEMP_WARN_THRESHOLD);
    dev->device_status = 2; /* 异常 */
    return 6;
  }

  /* 漏电检测告警 */
  if (dev->leakage_current > LEAKAGE_THRESHOLD) {
    snprintf(alarm_msg, msg_len,
             "LEAKAGE: 漏电电流%.1fmA超过阈值%.0fmA，已自动断电",
             dev->leakage_current, LEAKAGE_THRESHOLD);
    dev->light_status = 0; /* 自动断电 */
    dev->device_status = 0;
    return 7; /* 告警类型7: 漏电告警 */
  }

  /* 电压异常 */
  if (dev->voltage > VOLTAGE_HIGH) {
    snprintf(alarm_msg, msg_len, "VOLTAGE_HIGH: 电压%.1fV超过上限%.0fV",
             dev->voltage, VOLTAGE_HIGH);
    dev->device_status = 2;
    return 2; /* 告警类型2: 电压异常 */
  }
  if (dev->voltage < VOLTAGE_LOW && dev->light_status == 1) {
    snprintf(alarm_msg, msg_len, "VOLTAGE_LOW: 电压%.1fV低于下限%.0fV",
             dev->voltage, VOLTAGE_LOW);
    dev->device_status = 2;
    return 2;
  }

  /* 无告警时恢复正常 */
  if (dev->device_status == 2) {
    dev->device_status = 1;
  }
  return 0;
}

/* ============================================
 * 构建状态上报 JSON 数据包
 * 对应 MQTT Topic: streetlight/{uid}/status
 * ============================================ */
void build_status_payload(StreetlightDevice *dev, char *payload, int len) {
  snprintf(payload, len,
           "{"
           "\"deviceUid\":\"%s\","
           "\"voltage\":%.2f,"
           "\"current\":%.3f,"
           "\"temperature\":%.1f,"
           "\"brightness\":%d,"
           "\"lightStatus\":%d,"
           "\"onlineStatus\":%d,"
           "\"deviceStatus\":%d,"
           "\"leakageCurrent\":%.1f,"
           "\"power\":%d,"
           "\"runningHours\":%lu,"
           "\"hardwareModel\":\"%s\","
           "\"protectionRating\":\"%s\","
           "\"longitude\":%.4f,"
           "\"latitude\":%.4f,"
           "\"timestamp\":%ld"
           "}",
           dev->device_uid, dev->voltage, dev->current, dev->temperature,
           dev->brightness, dev->light_status, dev->online_status,
           dev->device_status, dev->leakage_current, dev->power,
           dev->running_seconds / 3600, dev->hardware_model,
           dev->protection_rating, dev->longitude, dev->latitude,
           (long)time(NULL));
}

/* ============================================
 * 构建告警上报 JSON 数据包
 * ============================================ */
void build_alarm_payload(StreetlightDevice *dev, int alarm_type,
                         const char *alarm_msg, char *payload, int len) {
  snprintf(payload, len,
           "{"
           "\"deviceUid\":\"%s\","
           "\"alarmType\":%d,"
           "\"message\":\"%s\","
           "\"voltage\":%.2f,"
           "\"current\":%.3f,"
           "\"temperature\":%.1f,"
           "\"leakageCurrent\":%.1f,"
           "\"timestamp\":%ld"
           "}",
           dev->device_uid, alarm_type, alarm_msg, dev->voltage, dev->current,
           dev->temperature, dev->leakage_current, (long)time(NULL));
}

/* ============================================
 * MQTT 消息到达回调
 * 处理平台下发的控制指令
 * ============================================ */
int message_arrived(void *context, char *topicName, int topicLen,
                    MQTTClient_message *message) {
  (void)context;
  (void)topicLen; /* 回调签名要求，未使用 */
  char payload[MAX_PAYLOAD_LEN];
  int len = message->payloadlen < MAX_PAYLOAD_LEN - 1 ? message->payloadlen
                                                      : MAX_PAYLOAD_LEN - 1;
  memcpy(payload, message->payload, len);
  payload[len] = '\0';

  printf("\n[MCU] 收到平台控制指令:\n");
  printf("      Topic: %s\n", topicName);
  printf("      Payload: %s\n", payload);

  /* 简单 JSON 解析 - 查找 action 字段 */
  char *action_ptr = strstr(payload, "\"action\":");
  if (action_ptr) {
    if (strstr(action_ptr, "TURN_ON")) {
      g_device.light_status = 1;
      /* 解析亮度参数 */
      char *bright_ptr = strstr(payload, "\"brightness\":");
      if (bright_ptr) {
        int brightness = atoi(bright_ptr + 13);
        if (brightness > 0 && brightness <= 100) {
          g_device.brightness = brightness;
        }
      } else {
        g_device.brightness = 100;
      }
      g_device.device_status = 1;
      printf("[MCU] >>> 执行开灯，亮度: %d%%\n", g_device.brightness);

    } else if (strstr(action_ptr, "TURN_OFF")) {
      g_device.light_status = 0;
      g_device.brightness = 0;
      printf("[MCU] >>> 执行关灯\n");

    } else if (strstr(action_ptr, "SET_BRIGHTNESS")) {
      char *bright_ptr = strstr(payload, "\"brightness\":");
      if (bright_ptr) {
        int brightness = atoi(bright_ptr + 13);
        if (brightness >= 0 && brightness <= 100) {
          g_device.brightness = brightness;
          g_device.light_status = (brightness > 0) ? 1 : 0;
          printf("[MCU] >>> 亮度调整为: %d%%\n", brightness);
        }
      }
    } else {
      printf("[MCU] >>> 未知指令，忽略\n");
    }
  }

  MQTTClient_freeMessage(&message);
  MQTTClient_free(topicName);
  return 1;
}

/* ============================================
 * MQTT 连接断开回调
 * ============================================ */
void connection_lost(void *context, char *cause) {
  (void)context; /* 回调签名要求，未使用 */
  printf("[MCU] ⚠ MQTT 连接断开: %s\n", cause ? cause : "未知原因");
  printf("[MCU] 设备将尝试重新连接...\n");
}

/* ============================================
 * 发布 MQTT 消息
 * ============================================ */
int publish_message(MQTTClient client, const char *topic, const char *payload) {
  MQTTClient_message msg = MQTTClient_message_initializer;
  MQTTClient_deliveryToken token;

  msg.payload = (void *)payload;
  msg.payloadlen = (int)strlen(payload);
  msg.qos = QOS;
  msg.retained = 0;

  int rc = MQTTClient_publishMessage(client, topic, &msg, &token);
  if (rc != MQTTCLIENT_SUCCESS) {
    printf("[MCU] ✗ 消息发布失败 (rc=%d)\n", rc);
    return -1;
  }

  rc = MQTTClient_waitForCompletion(client, token, TIMEOUT);
  return rc;
}

/* ============================================
 * 主函数 - 设备运行主循环
 * ============================================ */
int main(int argc, char *argv[]) {
  const char *broker_url = (argc > 1) ? argv[1] : DEFAULT_BROKER;
  const char *device_uid = (argc > 2) ? argv[2] : DEFAULT_DEVICE_ID;

  char client_id[128];
  char topic_status[MAX_TOPIC_LEN];
  char topic_alarm[MAX_TOPIC_LEN];
  char topic_control[MAX_TOPIC_LEN];
  char payload[MAX_PAYLOAD_LEN];
  char alarm_msg[256];

  /* 注册信号处理 */
  signal(SIGINT, signal_handler);
  signal(SIGTERM, signal_handler);

  /* 初始化随机数种子 */
  srand((unsigned int)time(NULL));

  /* 初始化设备 */
  printf("==============================================\n");
  printf("  城市智慧路灯 - 嵌入式控制器模拟程序 (C语言)\n");
  printf("  MQTT 物联网通信客户端\n");
  printf("==============================================\n\n");

  device_init(&g_device, device_uid);

  /* 构建 MQTT Topic */
  snprintf(topic_status, MAX_TOPIC_LEN, "streetlight/%s/status", device_uid);
  snprintf(topic_alarm, MAX_TOPIC_LEN, "streetlight/%s/alarm", device_uid);
  snprintf(topic_control, MAX_TOPIC_LEN, "streetlight/%s/control", device_uid);
  snprintf(client_id, sizeof(client_id), "%s%s", CLIENT_ID_PREFIX, device_uid);

  /* 创建 MQTT 客户端 */
  printf("\n[MQTT] 正在连接 Broker: %s\n", broker_url);
  int rc = MQTTClient_create(&g_client, broker_url, client_id,
                             MQTTCLIENT_PERSISTENCE_NONE, NULL);
  if (rc != MQTTCLIENT_SUCCESS) {
    printf("[MQTT] ✗ 创建客户端失败 (rc=%d)\n", rc);
    return EXIT_FAILURE;
  }

  /* 设置回调 */
  MQTTClient_setCallbacks(g_client, NULL, connection_lost, message_arrived,
                          NULL);

  /* 连接选项 */
  MQTTClient_connectOptions conn_opts = MQTTClient_connectOptions_initializer;
  conn_opts.keepAliveInterval = 60;
  conn_opts.cleansession = 1;
  conn_opts.connectTimeout = 10;

  /* 设置遗嘱消息 - 设备异常断连时通知管理平台 */
  MQTTClient_willOptions will_opts = MQTTClient_willOptions_initializer;
  char will_payload[256];
  snprintf(will_payload, sizeof(will_payload),
           "{\"deviceUid\":\"%s\",\"onlineStatus\":0,\"message\":"
           "\"设备异常离线\",\"timestamp\":%ld}",
           device_uid, (long)time(NULL));
  will_opts.topicName = topic_status;
  will_opts.message = will_payload;
  will_opts.qos = QOS;
  will_opts.retained = 0;
  conn_opts.will = &will_opts;

  /* 连接 Broker */
  rc = MQTTClient_connect(g_client, &conn_opts);
  if (rc != MQTTCLIENT_SUCCESS) {
    printf("[MQTT] ✗ 连接失败 (rc=%d)\n", rc);
    printf("[MQTT] 请确保 MQTT Broker 正在运行: %s\n", broker_url);
    printf("[MQTT] 安装 Mosquitto: brew install mosquitto && mosquitto -d\n");
    MQTTClient_destroy(&g_client);
    return EXIT_FAILURE;
  }
  printf("[MQTT] ✓ 连接成功!\n");

  /* 订阅控制指令 Topic */
  rc = MQTTClient_subscribe(g_client, topic_control, QOS);
  if (rc != MQTTCLIENT_SUCCESS) {
    printf("[MQTT] ✗ 订阅失败: %s\n", topic_control);
  } else {
    printf("[MQTT] ✓ 已订阅控制指令: %s\n", topic_control);
  }

  printf("\n[MCU] 设备开始运行，每 %d 秒上报一次状态...\n", REPORT_INTERVAL);
  printf("[MCU] 按 Ctrl+C 停止设备\n\n");

  /* ==========================================
   * 主循环 - 模拟 MCU 主循环
   * ========================================== */
  while (g_running) {
    /* 1. 更新传感器读数(模拟ADC采集) */
    device_update_sensors(&g_device);

    /* 2. 检查告警条件 */
    int alarm_type =
        device_check_alarms(&g_device, alarm_msg, sizeof(alarm_msg));

    /* 3. 上报设备状态 */
    build_status_payload(&g_device, payload, sizeof(payload));
    rc = publish_message(g_client, topic_status, payload);
    g_device.total_reports++;

    if (rc == MQTTCLIENT_SUCCESS) {
      printf("[%ld] 状态上报 #%lu | 电压:%.1fV 电流:%.3fA 温度:%.1f℃ "
             "亮度:%d%% 灯:%s 状态:%s\n",
             (long)time(NULL), g_device.total_reports, g_device.voltage,
             g_device.current, g_device.temperature, g_device.brightness,
             g_device.light_status ? "ON" : "OFF",
             g_device.device_status == 1   ? "正常"
             : g_device.device_status == 2 ? "异常"
                                           : "故障");
    }

    /* 4. 如果有告警，发送告警消息 */
    if (alarm_type > 0) {
      build_alarm_payload(&g_device, alarm_type, alarm_msg, payload,
                          sizeof(payload));
      publish_message(g_client, topic_alarm, payload);
      printf("[MCU] ⚠ 告警上报: [类型%d] %s\n", alarm_type, alarm_msg);
    }

    /* 5. 等待下次上报 */
    sleep(REPORT_INTERVAL);
  }

  /* ==========================================
   * 优雅关闭 - 发送离线状态
   * ========================================== */
  printf("\n[MCU] 设备关闭中...\n");

  g_device.online_status = 0;
  g_device.device_status = 3; /* 离线 */
  build_status_payload(&g_device, payload, sizeof(payload));
  publish_message(g_client, topic_status, payload);
  printf("[MCU] 已发送离线状态\n");

  /* 断开 MQTT 连接 */
  MQTTClient_unsubscribe(g_client, topic_control);
  MQTTClient_disconnect(g_client, TIMEOUT);
  MQTTClient_destroy(&g_client);

  printf("[MCU] 设备已安全关闭\n");
  printf("[MCU] 共运行 %lu 秒，上报状态 %lu 次\n", g_device.running_seconds,
         g_device.total_reports);

  return EXIT_SUCCESS;
}
