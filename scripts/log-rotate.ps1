# log-rotate.ps1 — compress any .log file in ./logs larger than MAX_KB
# and prune archives older than KEEP_DAYS. (PowerShell version of log-rotate.sh)
$ErrorActionPreference = 'Stop'

$LogDir   = Join-Path (Split-Path -Parent $PSScriptRoot) 'logs'
$MaxKb    = if ($env:MAX_KB) { [int]$env:MAX_KB } else { 1024 }
$KeepDays = if ($env:KEEP_DAYS) { [int]$env:KEEP_DAYS } else { 7 }
$Stamp    = Get-Date -Format 'yyyyMMdd_HHmmss'

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

$rotated = 0
Get-ChildItem $LogDir -Filter '*.log' | ForEach-Object {
    $sizeKb = [math]::Floor($_.Length / 1KB)
    if ($sizeKb -ge $MaxKb) {
        $archive = $_.FullName -replace '\.log$', "_${Stamp}.log.gz"
        $in = [IO.File]::OpenRead($_.FullName)
        $fs = [IO.File]::Create($archive)
        $gz = [IO.Compression.GzipStream]::new($fs, [IO.Compression.CompressionMode]::Compress)
        $in.CopyTo($gz); $gz.Dispose(); $fs.Dispose(); $in.Dispose()
        [IO.File]::WriteAllText($_.FullName, '')
        Write-Host "[rotate] $($_.Name) (${sizeKb}KB) -> $(Split-Path $archive -Leaf)"
        $script:rotated++
    } else {
        Write-Host "[rotate] skip $($_.Name) (${sizeKb}KB < ${MaxKb}KB)"
    }
}

Write-Host "[rotate] pruning archives older than $KeepDays days"
Get-ChildItem $LogDir -Filter '*.log.gz' |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$KeepDays) } |
    ForEach-Object { Write-Host "[rotate] delete $($_.Name)"; Remove-Item $_.FullName }

Write-Host "[rotate] done, $rotated file(s) rotated"
