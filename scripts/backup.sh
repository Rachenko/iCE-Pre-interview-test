#!/usr/bin/env bash
# backup.sh — dump the monitoring database inside the db container,
# compress it, and keep it under ./backups (mounted as /backups in the container).
set -euo pipefail

CONTAINER="${DB_CONTAINER:-pm-db}"
DB_NAME="${DB_NAME:-monitoring}"
DB_USER="${DB_USER:-postgres}"
BACKUP_DIR="$(cd "$(dirname "$0")/.." && pwd)/backups"
STAMP="$(date +%Y%m%d_%H%M%S)"
FILE="monitoring_${STAMP}.sql.gz"

mkdir -p "$BACKUP_DIR"

echo "[backup] dumping $DB_NAME from container $CONTAINER ..."
docker exec "$CONTAINER" pg_dump -U "$DB_USER" -d "$DB_NAME" --clean --if-exists \
  | gzip > "$BACKUP_DIR/$FILE"

echo "[backup] wrote $BACKUP_DIR/$FILE ($(du -h "$BACKUP_DIR/$FILE" | cut -f1))"

# Retention: keep the last 14 backups
ls -1t "$BACKUP_DIR"/monitoring_*.sql.gz 2>/dev/null | tail -n +15 | while IFS= read -r old; do
  rm -v "$old"
done
echo "[backup] done"
