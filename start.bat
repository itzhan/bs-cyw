@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion

:: ============================================
:: 城市智慧路灯管理信息系统 - Windows 启动脚本
:: ============================================

echo.
echo ╔══════════════════════════════════════════════════╗
echo ║     城市智慧路灯管理信息系统 - 启动脚本         ║
echo ╚══════════════════════════════════════════════════╝
echo.

:: 1. 清理旧进程
echo [1/3] 清理旧进程...
taskkill /F /IM "java.exe" /T 2>nul
echo       旧 Java 进程已清理

:: 杀掉占用 3000 端口的 Python 进程
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":3000" ^| findstr "LISTENING" 2^>nul') do (
    taskkill /F /PID %%a 2>nul
)
echo.

:: 2. 启动后端
echo [2/3] 启动后端服务 (Spring Boot)...
start "Smart Streetlight - Backend" cmd /k "cd /d %~dp0backend\swim-admin && mvn spring-boot:run"
echo       后端已在新窗口启动
echo.

:: 3. 启动前端
echo [3/3] 启动前端服务 (HTTP Server)...
start "Smart Streetlight - Frontend" cmd /k "cd /d %~dp0frontend && python -m http.server 3000"
echo       前端已在新窗口启动
echo.

:: 信息面板
echo ╔══════════════════════════════════════════════════════════════╗
echo ║                   系统访问信息                              ║
echo ╠══════════════════════════════════════════════════════════════╣
echo ║  前端地址:  http://localhost:3000/pages/login.html          ║
echo ║  后端API:   http://localhost:8080/api                       ║
echo ╠══════════════════════════════════════════════════════════════╣
echo ║  角色        用户名       密码       真实姓名               ║
echo ╠══════════════════════════════════════════════════════════════╣
echo ║  管理员      admin        123456     张明                   ║
echo ║  运维员      operator1    123456     李维                   ║
echo ║  运维员      operator2    123456     王运                   ║
echo ║  普通用户    user1        123456     赵市民                 ║
echo ║  普通用户    user2        123456     陈居民                 ║
echo ╠══════════════════════════════════════════════════════════════╣
echo ║  停止服务请运行: stop.bat                                   ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo 提示: 后端和前端已在独立窗口中启动，请等待后端编译完成后访问
echo.
pause
