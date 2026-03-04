#!/bin/bash
# ============================================
# 城市智慧路灯管理信息系统 - Mac 一键启动脚本
# ============================================

set -e

# --- 颜色定义 ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# --- 配置 ---
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/backend/swim-admin"
FRONTEND_DIR="$SCRIPT_DIR/frontend"
SQL_DIR="$BACKEND_DIR/sql"
DB_NAME="smart_streetlight"
DB_USER="root"
DB_PASS="ab123168"
BACKEND_PORT=8080
FRONTEND_PORT=3000
BACKEND_LOG="/tmp/smartlight-backend.log"
FRONTEND_LOG="/tmp/smartlight-frontend.log"

# --- 打印横幅 ---
echo ""
echo -e "${CYAN}${BOLD}╔══════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}${BOLD}║     🏙️  城市智慧路灯管理信息系统 - 启动脚本     ║${NC}"
echo -e "${CYAN}${BOLD}╚══════════════════════════════════════════════════╝${NC}"
echo ""

# ============================================
# 1. 依赖检查
# ============================================
echo -e "${BLUE}[1/5]${NC} 🔍 检查系统依赖..."

MISSING=0
for cmd in java mvn mysql python3; do
    if command -v $cmd &> /dev/null; then
        echo -e "  ${GREEN}✅${NC} $cmd $(command -v $cmd)"
    else
        echo -e "  ${RED}❌${NC} $cmd 未安装"
        MISSING=1
    fi
done

if [ $MISSING -eq 1 ]; then
    echo -e "${RED}⛔ 缺少必要依赖，请先安装后再试。${NC}"
    exit 1
fi
echo ""

# ============================================
# 2. 清理旧进程
# ============================================
echo -e "${BLUE}[2/5]${NC} 🧹 清理旧进程..."

if lsof -ti :$BACKEND_PORT > /dev/null 2>&1; then
    echo -e "  ${YELLOW}⚠️${NC}  端口 $BACKEND_PORT 被占用，正在清理..."
    lsof -ti :$BACKEND_PORT | xargs kill -9 2>/dev/null || true
    sleep 1
else
    echo -e "  ${GREEN}✅${NC} 端口 $BACKEND_PORT 空闲"
fi

if lsof -ti :$FRONTEND_PORT > /dev/null 2>&1; then
    echo -e "  ${YELLOW}⚠️${NC}  端口 $FRONTEND_PORT 被占用，正在清理..."
    lsof -ti :$FRONTEND_PORT | xargs kill -9 2>/dev/null || true
    sleep 1
else
    echo -e "  ${GREEN}✅${NC} 端口 $FRONTEND_PORT 空闲"
fi
echo ""

# ============================================
# 3. 数据库导入
# ============================================
echo -e "${BLUE}[3/5]${NC} 🗄️  检查数据库..."

# 测试 MySQL 连接
if ! mysql -u"$DB_USER" -p"$DB_PASS" -e "SELECT 1" &> /dev/null; then
    echo -e "  ${RED}❌ 无法连接 MySQL（用户: $DB_USER，密码: $DB_PASS）${NC}"
    echo -e "  ${YELLOW}请检查 MySQL 是否运行，以及账号密码是否正确。${NC}"
    exit 1
fi

# 检查数据库是否已存在
if mysql -u"$DB_USER" -p"$DB_PASS" -e "USE $DB_NAME" &> /dev/null 2>&1; then
    # 数据库存在，检查是否有数据
    TABLE_COUNT=$(mysql -u"$DB_USER" -p"$DB_PASS" -N -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='$DB_NAME'" 2>/dev/null)
    if [ "$TABLE_COUNT" -gt 0 ]; then
        echo -e "  ${GREEN}✅${NC} 数据库 ${BOLD}$DB_NAME${NC} 已存在（${TABLE_COUNT} 张表），跳过导入"
    else
        echo -e "  ${YELLOW}⚠️${NC}  数据库存在但为空，正在导入数据..."
        mysql -u"$DB_USER" -p"$DB_PASS" < "$SQL_DIR/init.sql"
        mysql -u"$DB_USER" -p"$DB_PASS" < "$SQL_DIR/data.sql"
        echo -e "  ${GREEN}✅${NC} 数据库导入完成！"
    fi
else
    echo -e "  ${YELLOW}📦${NC} 数据库 ${BOLD}$DB_NAME${NC} 不存在，正在创建并导入..."
    mysql -u"$DB_USER" -p"$DB_PASS" < "$SQL_DIR/init.sql"
    echo -e "  ${GREEN}✅${NC} 表结构创建完成"
    mysql -u"$DB_USER" -p"$DB_PASS" < "$SQL_DIR/data.sql"
    echo -e "  ${GREEN}✅${NC} 测试数据导入完成"
fi
echo ""

# ============================================
# 4. 启动服务
# ============================================
echo -e "${BLUE}[4/5]${NC} 🚀 启动服务..."

# 清空旧日志
> "$BACKEND_LOG"
> "$FRONTEND_LOG"

# 启动后端
echo -e "  ${CYAN}▶${NC}  启动后端服务 (Spring Boot)..."
cd "$BACKEND_DIR"
mvn spring-boot:run > "$BACKEND_LOG" 2>&1 &
BACKEND_PID=$!
echo -e "  ${GREEN}✅${NC} 后端已启动 (PID: $BACKEND_PID)"

# 启动前端
echo -e "  ${CYAN}▶${NC}  启动前端服务 (HTTP Server)..."
cd "$FRONTEND_DIR"
python3 -m http.server $FRONTEND_PORT > "$FRONTEND_LOG" 2>&1 &
FRONTEND_PID=$!
echo -e "  ${GREEN}✅${NC} 前端已启动 (PID: $FRONTEND_PID)"
cd "$SCRIPT_DIR"
echo ""

# ============================================
# 5. 打印信息面板
# ============================================
echo -e "${BLUE}[5/5]${NC} 📋 系统信息"
echo ""
echo -e "${CYAN}${BOLD}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}${BOLD}║                   🏙️  系统访问信息                          ║${NC}"
echo -e "${CYAN}${BOLD}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🌐 前端地址:  ${GREEN}http://localhost:${FRONTEND_PORT}/pages/login.html${NC}   ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🔧 后端API:   ${GREEN}http://localhost:${BACKEND_PORT}/api${NC}                ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${CYAN}${BOLD}║${NC}  ${BOLD}角色${NC}        ${BOLD}用户名${NC}       ${BOLD}密码${NC}       ${BOLD}真实姓名${NC}              ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🔴 管理员    admin        123456     张明                  ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🟡 运维员    operator1    123456     李维                  ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🟡 运维员    operator2    123456     王运                  ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🟢 普通用户  user1        123456     赵市民                ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}║${NC}  🟢 普通用户  user2        123456     陈居民                ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${CYAN}${BOLD}║${NC}  📝 停止服务请运行: ${YELLOW}./stop.sh${NC}                             ${CYAN}${BOLD}║${NC}"
echo -e "${CYAN}${BOLD}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# ============================================
# 等待后端启动完成
# ============================================
echo -e "${YELLOW}⏳ 等待后端启动完成...${NC}"
for i in $(seq 1 60); do
    if curl -s http://localhost:$BACKEND_PORT/api/auth/login > /dev/null 2>&1; then
        echo -e "${GREEN}✅ 后端启动成功！${NC}"
        break
    fi
    if ! kill -0 $BACKEND_PID 2>/dev/null; then
        echo -e "${RED}❌ 后端启动失败，请查看日志: $BACKEND_LOG${NC}"
        tail -20 "$BACKEND_LOG"
        exit 1
    fi
    sleep 2
    echo -ne "\r${YELLOW}⏳ 等待后端启动完成... (${i}/60)${NC}"
done
echo ""

# ============================================
# 实时日志输出
# ============================================
echo -e "${CYAN}${BOLD}════════════════════ 📜 实时日志 ════════════════════${NC}"
echo -e "${YELLOW}按 Ctrl+C 可停止查看日志（服务不会停止）${NC}"
echo ""

tail -f "$BACKEND_LOG" "$FRONTEND_LOG"
