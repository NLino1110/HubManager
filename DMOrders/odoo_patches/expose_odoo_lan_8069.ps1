# Reenvia puerto 8069 (Odoo en WSL) a la red LAN de Windows.
# Ejecutar como Administrador.

$ErrorActionPreference = "Stop"

$wslIp = (wsl hostname -I).Trim().Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)[0]
if ([string]::IsNullOrWhiteSpace($wslIp)) {
    Write-Host "No se pudo obtener la IP de WSL." -ForegroundColor Red
    exit 1
}

Write-Host "WSL IP: $wslIp"

netsh interface portproxy delete v4tov4 listenport=8069 listenaddress=0.0.0.0 2>$null | Out-Null
netsh interface portproxy add v4tov4 listenaddress=0.0.0.0 listenport=8069 connectaddress=$wslIp connectport=8069

Write-Host "`nPortproxy actual:"
netsh interface portproxy show all

$rule = Get-NetFirewallRule -DisplayName "Odoo 8069 LAN" -ErrorAction SilentlyContinue
if (-not $rule) {
    New-NetFirewallRule -DisplayName "Odoo 8069 LAN" -Direction Inbound -Protocol TCP -LocalPort 8069 -Action Allow | Out-Null
    Write-Host "Regla de firewall creada: Odoo 8069 LAN"
} else {
    Enable-NetFirewallRule -DisplayName "Odoo 8069 LAN"
    Write-Host "Regla de firewall ya existia; habilitada."
}

$lanIp = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.InterfaceAlias -match 'Wi-Fi|WLAN|Wireless' -and $_.IPAddress -notlike '169.*' } |
    Select-Object -First 1 -ExpandProperty IPAddress)

Write-Host "`nListo. En la tablet usa:"
if ($lanIp) {
    Write-Host "  http://${lanIp}:8069" -ForegroundColor Green
} else {
    Write-Host "  http://IP_DE_TU_LAPTOP:8069" -ForegroundColor Green
}

Write-Host "`nPrueba desde esta ventana:"
try {
    $r = Invoke-WebRequest -Uri "http://127.0.0.1:8069/web/login" -UseBasicParsing -TimeoutSec 8
    Write-Host "  localhost OK ($($r.StatusCode))"
} catch {
    Write-Host "  localhost FALLO: $($_.Exception.Message)" -ForegroundColor Yellow
}

if ($lanIp) {
    try {
        $r2 = Invoke-WebRequest -Uri "http://$lanIp`:8069/web/login" -UseBasicParsing -TimeoutSec 8
        Write-Host "  LAN ($lanIp) OK ($($r2.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "  LAN ($lanIp) FALLO: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`nNota: si reinicias WSL, vuelve a ejecutar este script (la IP de WSL cambia)."
pause
