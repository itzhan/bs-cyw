# 城市智慧路灯管理信息系统 - C# 后端

> ASP.NET Core 8 Web API，完全替代原 Java Spring Boot 后端，API 路径 100% 兼容。

## 技术栈

- **框架**: ASP.NET Core 8 Web API
- **ORM**: Entity Framework Core 8 + Pomelo MySQL
- **认证**: JWT Bearer Authentication
- **密码**: BCrypt.Net-Next（与 Java BCrypt 兼容）
- **IoT**: MQTTnet 4.x

## 环境要求

- .NET 8 SDK
- MySQL 8.0+
- （可选）MQTT Broker

## Docker 一键部署（推荐）

项目根目录已提供前端 + C# 后端 + C 设备模拟器的一键编排：

```bash
cd /Users/itzhan/Desktop/毕业设计/陈勇忘
DOCKER_BUILDKIT=0 docker compose up -d --build
```

详细说明见：`/Users/itzhan/Desktop/毕业设计/陈勇忘/README-docker-csharp-c.md`

## 快速启动

### 1. 安装 .NET 8 SDK

从 https://dotnet.microsoft.com/download/dotnet/8.0 下载安装。

### 2. 配置数据库

编辑 `SmartStreetlight.Api/appsettings.json`，修改数据库连接：

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=smart_streetlight;User=root;Password=你的密码;CharSet=utf8mb4;"
}
```

### 3. 初始化数据库

使用原有的 `init.sql` 和 `data.sql` 初始化数据库（与 Java 版共用同一个数据库）。

### 4. 运行

```bash
cd SmartStreetlight.Api
dotnet restore
dotnet run
```

服务将在 `http://localhost:8080` 启动。

### 5. 测试登录

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"123456"}'
```

## 项目结构

```
SmartStreetlight.Api/
├── Program.cs                    # 应用入口（DI、JWT、CORS、EF Core）
├── appsettings.json              # 配置文件
├── Common/                       # 通用响应类
│   ├── Result.cs
│   └── PageResult.cs
├── Models/
│   ├── Entities/                 # 数据库实体（13个）
│   └── DTOs/                     # 数据传输对象（8个）
├── Data/
│   └── AppDbContext.cs           # EF Core DbContext
├── Services/
│   ├── JwtService.cs             # JWT 生成
│   └── MqttPublishService.cs     # MQTT 上下行（下发 + 设备上报订阅入库）
└── Controllers/                  # API 控制器（14个）
    ├── AuthController.cs
    ├── StreetlightController.cs
    ├── AreaCabinetRoleController.cs
    ├── AlarmWorkOrderController.cs
    ├── ControlStrategyController.cs
    ├── StatisticsController.cs
    ├── RepairAnnouncementController.cs
    └── UserMqttController.cs
```

## API 对照表

| 功能 | 路径 | 方法 |
|------|------|------|
| 登录 | `/api/auth/login` | POST |
| 注册 | `/api/auth/register` | POST |
| 路灯列表 | `/api/streetlights` | GET |
| 路灯全量 | `/api/streetlights/all` | GET |
| 区域树 | `/api/areas/tree` | GET |
| 电柜列表 | `/api/cabinets` | GET |
| 告警列表 | `/api/alarms` | GET |
| 工单列表 | `/api/work-orders` | GET |
| 执行控制 | `/api/control/execute` | POST |
| 策略管理 | `/api/strategies` | GET/POST/PUT/DELETE |
| 数据统计 | `/api/statistics/overview` | GET |
| 报修管理 | `/api/repair-reports` | GET/POST |
| 公告管理 | `/api/announcements` | GET/POST/PUT/DELETE |
| 用户管理 | `/api/users` | GET/PUT/DELETE |
| MQTT 状态 | `/api/mqtt/status` | GET |

## 测试账号

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | 123456 | 系统管理员 |
| operator1 | 123456 | 运维人员 |
| user1 | 123456 | 普通用户 |
