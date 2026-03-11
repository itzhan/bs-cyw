@echo off
chcp 65001 >nul 2>&1

echo 正在停止服务...
cd /d "%~dp0"
docker compose down

echo 所有服务已停止。
echo.
pause
