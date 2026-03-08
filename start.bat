@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion

:: ============================================
:: 城市智慧路灯管理信息系统 - Windows 一键启动
:: 双击即可启动，无需安装任何开发环境
:: 前提：已安装 Docker Desktop
:: ============================================

echo.
echo ╔══════════════════════════════════════════════════╗
echo ║   城市智慧路灯管理信息系统 - Docker 一键启动    ║
echo ╚══════════════════════════════════════════════════╝
echo.

:: ---------------------------------------------------
:: 1. 检查 Docker 是否可用
:: ---------------------------------------------------
echo [1/3] 检查 Docker 环境...

where docker >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo   ╔══════════════════════════════════════════════╗
    echo   ║  未检测到 Docker，请先安装 Docker Desktop   ║
    echo   ║  下载地址: https://www.docker.com/products/  ║
    echo   ║            docker-desktop                    ║
    echo   ╚══════════════════════════════════════════════╝
    echo.
    pause
    exit /b 1
)

:: 检查 Docker 引擎是否运行
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo       Docker Desktop 未运行，正在尝试启动...
    start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    echo       等待 Docker Desktop 启动（约 30 秒）...
    timeout /t 30 /nobreak >nul
    docker info >nul 2>&1
    if %errorlevel% neq 0 (
        echo       Docker Desktop 仍未就绪，请手动启动后再试。
        pause
        exit /b 1
    )
)
echo       Docker 环境就绪
echo.

:: ---------------------------------------------------
:: 2. 构建并启动所有容器
:: ---------------------------------------------------
echo [2/3] 构建并启动服务（首次启动需 3-5 分钟）...
echo.

cd /d "%~dp0"
docker compose up -d --build

if %errorlevel% neq 0 (
    echo.
    echo   启动失败，请检查错误信息。
    pause
    exit /b 1
)
echo.

:: ---------------------------------------------------
:: 3. 显示信息面板
:: ---------------------------------------------------
echo [3/3] 启动完成！
echo.
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
echo ║  停止服务: 双击 stop.bat 或运行 docker compose down        ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo 提示: 首次启动后端编译约需 2-3 分钟，请稍等后再访问前端页面
echo.
pause
