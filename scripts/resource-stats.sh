#!/usr/bin/env bash
# resource-stats.sh — show CPU / memory usage of the running containers.
set -euo pipefail

COMPOSE_DIR="$(cd "$(dirname "$0")/.." && pwd)"

echo "== Live resource usage (one snapshot) =="
docker stats --no-stream --format \
  'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.PIDs}}' \
  $(docker compose -f "$COMPOSE_DIR/docker-compose.yml" ps -q)
