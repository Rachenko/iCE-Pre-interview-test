# backup.ps1 — dump the monitoring database inside the db container,
# compress it, and keep it under ./backups (PowerShell version of backup.sh)
$ErrorActionPreference = 'Stop'

$Container = if ($env:DB_CONTAINER) { $env:DB_CONTAINER } else { 'pm-db' }
$DbName    = if ($env:DB_NAME) { $env:DB_NAME } else { 'monitoring' }
$DbUser    = if ($env:DB_USER) { $env:DB_USER } else { 'postgres' }
$BackupDir = Join-Path (Split-Path -Parent $PSScriptRoot) 'backups'
$Stamp     = Get-Date -Format 'yyyyMMdd_HHmmss'
$File      = "monitoring_${Stamp}.sql.gz"

New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

Write-Host "[backup] dumping $DbName from container $Container ..."
$dump = docker exec $Container pg_dump -U $DbUser -d $DbName --clean --if-exists
if ($LASTEXITCODE -ne 0) { throw "pg_dump failed" }
$outPath = Join-Path $BackupDir $File
$in = [IO.MemoryStream]::new([Text.Encoding]::UTF8.GetBytes(($dump -join "`n") + "`n"))
$fs = [IO.File]::Create($outPath)
$gz = [IO.Compression.GzipStream]::new($fs, [IO.Compression.CompressionMode]::Compress)
$in.CopyTo($gz); $gz.Dispose(); $fs.Dispose(); $in.Dispose()

$size = '{0:N0} KB' -f ((Get-Item $outPath).Length / 1KB)
Write-Host "[backup] wrote $outPath ($size)"

# Retention: keep the last 14 backups
Get-ChildItem $BackupDir -Filter 'monitoring_*.sql.gz' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -Skip 14 |
    ForEach-Object { Write-Host "[backup] removing old backup $($_.Name)"; Remove-Item $_.FullName }

Write-Host "[backup] done"
