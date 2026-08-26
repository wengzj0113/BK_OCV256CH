$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$files = @('frmTest.vb', 'MDIForm.vb', 'Module1.vb', 'frmDebug.vb', 'SocketClient.vb', 'Cls_Sock.vb')
$forbidden = @('TcpClient', 'BeginConnect', 'SocketType.Stream', 'SCMIpAdd', 'SCM_port', 'CheckTcp_SCM', 'ConnetTcp_SCM')
$hits = @()
foreach ($name in $files) {
    $path = Join-Path $root $name
    if (-not (Test-Path $path)) { continue }
    $lineNo = 0
    foreach ($line in Get-Content -LiteralPath $path) {
        $lineNo++
        if ($line.TrimStart().StartsWith("'")) { continue }
        foreach ($token in $forbidden) {
            if ($line -match [regex]::Escape($token)) {
                $hits += "${name}:${lineNo}: $token"
            }
        }
    }
}
if ($hits.Count -gt 0) {
    Write-Host 'MIDDLE_MACHINE_SERIAL_ONLY: FAIL'
    $hits | Select-Object -Unique | ForEach-Object { Write-Host $_ }
    exit 1
}
Write-Host 'MIDDLE_MACHINE_SERIAL_ONLY: PASS'
