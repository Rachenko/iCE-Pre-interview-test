# resource-stats.ps1 — show CPU / memory usage of the running containers.
# (PowerShell version of resource-stats.sh)
$ErrorActionPreference = 'Stop'

$ComposeDir = Split-Path -Parent $PSScriptRoot
$ids = docker compose -f "$ComposeDir/docker-compose.yml" ps -q
if (-not $ids) { Write-Host "No running containers"; exit 0 }

Write-Host "== Live resource usage (one snapshot) =="
docker stats --no-stream --format 'table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.NetIO}}\t{{.PIDs}}' $ids
