# ============================================
# 城市智慧路灯 - 嵌入式设备模拟器 Makefile
# ============================================

CC = gcc
PAHO_PREFIX = $(shell brew --prefix libpaho-mqtt 2>/dev/null || echo /usr/local)
CFLAGS = -Wall -Wextra -std=c99 -O2 -I$(PAHO_PREFIX)/include
LDFLAGS = -L$(PAHO_PREFIX)/lib -lpaho-mqtt3c -lm -lpthread

TARGET = streetlight_device
SRC = streetlight_device.c

.PHONY: all clean run run-multi help install-deps

all: $(TARGET)

$(TARGET): $(SRC)
	$(CC) $(CFLAGS) -o $@ $< $(LDFLAGS)
	@echo "✓ 编译成功: $(TARGET)"

# 运行单个设备模拟
run: $(TARGET)
	./$(TARGET) tcp://localhost:1883 DEV-HYL-001

# 运行多个设备模拟(后台)
run-multi: $(TARGET)
	@echo "启动 3 个设备模拟器..."
	./$(TARGET) tcp://localhost:1883 DEV-HYL-001 &
	./$(TARGET) tcp://localhost:1883 DEV-HYL-002 &
	./$(TARGET) tcp://localhost:1883 DEV-JSL-014 &
	@echo "3 个设备已在后台运行，使用 'make stop' 停止"

# 停止所有模拟器
stop:
	@pkill -f streetlight_device 2>/dev/null || echo "没有运行中的模拟器"

# 安装依赖
install-deps:
	@echo "安装 Eclipse Paho MQTT C 客户端库..."
	@if [ "$$(uname)" = "Darwin" ]; then \
		brew install eclipse-paho-mqtt-c; \
	else \
		sudo apt-get install -y libpaho-mqtt-dev; \
	fi
	@echo "安装 Mosquitto MQTT Broker (可选)..."
	@if [ "$$(uname)" = "Darwin" ]; then \
		brew install mosquitto; \
	else \
		sudo apt-get install -y mosquitto mosquitto-clients; \
	fi

clean:
	rm -f $(TARGET)

help:
	@echo "用法:"
	@echo "  make              - 编译设备模拟器"
	@echo "  make run          - 运行单个设备"
	@echo "  make run-multi    - 后台运行 3 个设备"
	@echo "  make stop         - 停止所有设备"
	@echo "  make install-deps - 安装依赖库"
	@echo "  make clean        - 清理编译产物"
