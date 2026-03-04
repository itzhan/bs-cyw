@echo off
chcp 65001 >nul 2>&1

:: ============================================
:: 城市智慧路灯管理信息系统 - Windows 停止脚本
:: ============================================

echo.
echo ╔══════════════════════════════════════════════════╗
echo ║     城市智慧路灯管理信息系统 - 停止服务         ║
echo ╚══════════════════════════════════════════════════╝
echo.

echo 正在停止后端服务...
taskkill /F /IM "java.exe" /T 2>nul
echo 后端服务已停止

echo 正在停止前端服务...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":3000" ^| findstr "LISTENING" 2^>nul') do (
    taskkill /F /PID %%a 2>nul
)
echo 前端服务已停止

echo.
echo 所有服务已停止
echo.
pause
