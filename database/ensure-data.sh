#!/bin/bash
# 每次 MySQL 容器启动时检测数据库，若表为空则自动导入数据
# 此脚本由 docker-compose 的 backend 容器健康检查后触发无关
# 它作为 MySQL 容器的自定义启动脚本

set -e

HOST="mysql"
USER="root"
PASS="root123"
DB="smart_streetlight"

echo "[ensure-data] Waiting for MySQL to be ready..."
for i in $(seq 1 30); do
    if mysqladmin ping -h"$HOST" -u"$USER" -p"$PASS" --silent 2>/dev/null; then
        break
    fi
    sleep 2
done

# 检查 streetlight 表是否有数据
ROW_COUNT=$(mysql -h"$HOST" -u"$USER" -p"$PASS" -N -e "SELECT COUNT(*) FROM ${DB}.streetlight;" 2>/dev/null || echo "0")

if [ "$ROW_COUNT" -gt "0" ]; then
    echo "[ensure-data] Database already has data ($ROW_COUNT streetlights). Skipping seed import."
else
    echo "[ensure-data] Database is empty. Importing schema and data..."
    mysql -h"$HOST" -u"$USER" -p"$PASS" "$DB" < /sql/init.sql
    mysql -h"$HOST" -u"$USER" -p"$PASS" "$DB" < /sql/data.sql
    echo "[ensure-data] Import completed successfully."
fi

# 始终尝试应用 v2 迁移(幂等: IF NOT EXISTS),修复旧库并清理爆炸告警
if [ -f /sql/migration_v2.sql ]; then
    echo "[ensure-data] Applying migration_v2.sql (idempotent)..."
    mysql -h"$HOST" -u"$USER" -p"$PASS" "$DB" < /sql/migration_v2.sql 2>/dev/null || echo "[ensure-data] migration_v2 skipped (maybe already applied)"
fi
