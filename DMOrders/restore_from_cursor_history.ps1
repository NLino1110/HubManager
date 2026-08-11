# Restaura archivos de HubManager desde el historial local de Cursor
# (version anterior al git restore del 11/08/2026 ~08:28)
#
# Uso: powershell -ExecutionPolicy Bypass -File restore_from_cursor_history.ps1

$historyRoot = "$env:APPDATA\Cursor\User\History"
$restored = 0
$errors = @()

Get-ChildItem $historyRoot -Directory | ForEach-Object {
    $entriesPath = Join-Path $_.FullName 'entries.json'
    if (-not (Test-Path $entriesPath)) { return }

    try { $meta = Get-Content $entriesPath -Raw | ConvertFrom-Json } catch { return }
    if ($meta.resource -notmatch 'HubManager') { return }

    $path = ($meta.resource -replace '^file:///', '')
    $path = [Uri]::UnescapeDataString($path) -replace '/', '\'

    $entries = @($meta.entries)
    if ($entries.Count -eq 0) { return }

    # Ultima version ANTES del git restore (ignorar "Undo Create Diff")
    $pick = $entries |
        Where-Object { $_.source -ne 'Undo Create Diff' } |
        Sort-Object { [int64]$_.timestamp } -Descending |
        Select-Object -First 1

    if (-not $pick) { return }

    $srcFile = Join-Path $_.FullName $pick.id
    if (-not (Test-Path $srcFile)) {
        $errors += "Snapshot no encontrado: $srcFile"
        return
    }

    $targetDir = Split-Path $path -Parent
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }

    Copy-Item -Path $srcFile -Destination $path -Force
    $ts = [DateTimeOffset]::FromUnixTimeMilliseconds([int64]$pick.timestamp).LocalDateTime
    Write-Host "OK  $path  ($ts)"
    $restored++
}

Write-Host ""
Write-Host "Restaurados: $restored archivos"
if ($errors.Count -gt 0) {
    Write-Host "Errores:"
    $errors | ForEach-Object { Write-Host "  $_" }
}
