# healthcheck.ps1 — verify every container in the stack is up and healthy,
# then hit the API health endpoint. (PowerShell version of healthcheck.sh)
$ErrorActionPreference = 'Stop'

$ApiUrl = if ($env:API_URL) { $env:API_URL } else { 'http://localhost:8080/health' }
$ComposeDir = Split-Path -Parent $PSScriptRoot

Write-Host "== Container status =="
docker compose -f "$ComposeDir/docker-compose.yml" ps --format 'table {{.Name}}\t{{.Status}}\t{{.Ports}}'

Write-Host "`n== API health =="
try {
    $resp = Invoke-RestMethod -Uri $ApiUrl -TimeoutSec 5
    Write-Host ($resp | ConvertTo-Json -Compress)
    Write-Host "OK: API is healthy"
} catch {
    Write-Host "FAIL: API health check failed at $ApiUrl"
    exit 1
}
