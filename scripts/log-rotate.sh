#!/usr/bin/env bash
# log-rotate.sh — simple log rotation demo: compress any .log file in ./logs
# larger than MAX_KB and prune archives older than KEEP_DAYS.
set -euo pipefail

LOG_DIR="$(cd "$(dirname "$0")/.." && pwd)/logs"
MAX_KB="${MAX_KB:-1024}"      # rotate logs bigger than 1 MB
KEEP_DAYS="${KEEP_DAYS:-7}"   # delete archives older than a week
STAMP="$(date +%Y%m%d_%H%M%S)"

mkdir -p "$LOG_DIR"

rotated=0
for f in "$LOG_DIR"/*.log; do
  [ -e "$f" ] || continue
  size_kb=$(( $(wc -c < "$f") / 1024 ))
  if [ "$size_kb" -ge "$MAX_KB" ]; then
    gzip -c "$f" > "${f%.log}_${STAMP}.log.gz"
    : > "$f"   # truncate the live log file
    echo "[rotate] $f (${size_kb}KB) -> ${f%.log}_${STAMP}.log.gz"
    rotated=$((rotated + 1))
  else
    echo "[rotate] skip $f (${size_kb}KB < ${MAX_KB}KB)"
  fi
done

echo "[rotate] pruning archives older than ${KEEP_DAYS} days"
find "$LOG_DIR" -name '*.log.gz' -mtime "+$KEEP_DAYS" -print -delete

echo "[rotate] done, $rotated file(s) rotated"
