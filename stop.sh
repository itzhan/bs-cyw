#!/bin/bash
# ============================================
# 城市智慧路灯管理信息系统 - Mac 停止脚本
# ============================================

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

BACKEND_PORT=8080
FRONTEND_PORT=3000

echo ""
echo -e "${CYAN}${BOLD}╔══════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}${BOLD}║     🛑 城市智慧路灯管理系统 - 停止服务          ║${NC}"
echo -e "${CYAN}${BOLD}╚══════════════════════════════════════════════════╝${NC}"
echo ""

# 停止后端
if lsof -ti :$BACKEND_PORT > /dev/null 2>&1; then
    echo -e "  ${YELLOW}▶${NC}  正在停止后端服务 (端口 $BACKEND_PORT)..."
    lsof -ti :$BACKEND_PORT | xargs kill -9 2>/dev/null || true
    echo -e "  ${GREEN}✅${NC} 后端服务已停止"
else
    echo -e "  ${GREEN}✅${NC} 后端服务未运行"
fi

# 停止前端
if lsof -ti :$FRONTEND_PORT > /dev/null 2>&1; then
    echo -e "  ${YELLOW}▶${NC}  正在停止前端服务 (端口 $FRONTEND_PORT)..."
    lsof -ti :$FRONTEND_PORT | xargs kill -9 2>/dev/null || true
    echo -e "  ${GREEN}✅${NC} 前端服务已停止"
else
    echo -e "  ${GREEN}✅${NC} 前端服务未运行"
fi

echo ""
echo -e "${GREEN}${BOLD}✅ 所有服务已停止${NC}"
echo ""
