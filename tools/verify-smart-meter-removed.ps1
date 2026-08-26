$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$files = @(
    'frmInit.vb', 'frmInit.Designer.vb', 'MDIForm.vb', 'MDIForm.Designer.vb',
    'Module1.vb', 'FrmMesTask.vb', 'FrmMesTask.Designer.vb'
)

foreach ($file in $files) {
    $text = Get-Content (Join-Path $root $file) -Raw
    if ($file -eq 'Module1.vb') {
        $text = [regex]::Replace($text, '(?s)#If False Then.*?#End If', '')
    }
    if ($text -match 'EmmPort|EmmRate|Com_EmmCard|ReadEnergeriEMM3200|CheckCom_EmmCard|GetTotalEnergeValue|ConnectToEMM|energyUpload|chkEnergyUpload|ELECTRIC') {
        throw "FAIL: smart-meter code remains in $file"
    }
}

Write-Output 'SMART_METER_REMOVED: PASS'
