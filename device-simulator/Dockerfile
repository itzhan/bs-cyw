FROM debian:bookworm-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends gcc libc6-dev libpaho-mqtt-dev ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY streetlight_device.c /app/streetlight_device.c

RUN gcc -Wall -Wextra -std=c99 -O2 -o /app/streetlight_device /app/streetlight_device.c -lpaho-mqtt3c -lm -lpthread

ENTRYPOINT ["/app/streetlight_device"]
CMD ["tcp://mqtt:1883", "DEV-HYL-001"]
