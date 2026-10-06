#!/usr/bin/env bash
# healthcheck.sh — verify every container in the stack is up and healthy,
# then hit the API health endpoint.
set -euo pipefail

API_URL="${API_URL:-http://localhost:8080/health}"
COMPOSE_DIR="$(cd "$(dirname "$0")/.." && pwd)"

echo "== Container status =="
docker compose -f "$COMPOSE_DIR/docker-compose.yml" ps --format \
  'table {{.Name}}\t{{.Status}}\t{{.Ports}}'

echo
echo "== API health =="
if curl -fsS --max-time 5 "$API_URL"; then
  echo -e "\nOK: API is healthy"
else
  echo -e "\nFAIL: API health check failed at $API_URL" >&2
  exit 1
fi
