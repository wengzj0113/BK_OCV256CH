$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$init = Get-Content (Join-Path $root 'frmInit.vb') -Raw
$designer = Get-Content (Join-Path $root 'frmInit.Designer.vb') -Raw
$module = Get-Content (Join-Path $root 'Module1.vb') -Raw

function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) { throw "FAIL: $message" }
}

Assert-Contains $designer 'ChbPLCIP' 'PLC TCP checkbox is missing'
Assert-Contains $designer 'ChbPlcCom' 'PLC COM checkbox is missing'
Assert-Contains $init 'ChbPLCIP_CheckedChanged' 'PLC TCP mutual exclusion handler is missing'
Assert-Contains $init 'ChbPlcCom_CheckedChanged' 'PLC COM mutual exclusion handler is missing'
Assert-Contains $init 'UpdatePlcTransportControls' 'PLC transport UI refresh is missing'
Assert-Contains $init 'PLCMode|PLCComPort|PLCComRate|PLCIP|PLCTcpPort' 'PLC settings are not saved or loaded'
Assert-Contains $module 'PLCMode|PLCComPort|PLCComRate|PLCIP|PLCTcpPort' 'PLC settings are not defined in Initset'

Write-Output 'PLC_SETTINGS: PASS'
