$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$formType = $assembly.GetType('software.frmTest', $true)
$format = $formType.GetMethod('FormatLocalResultRows', [Reflection.BindingFlags]'Static,Public,NonPublic')
if ($null -eq $format) { throw 'Local result formatting is not available for all three OCV stages.' }
$cellType = $assembly.GetType('software.Module1+Bat_info', $true)
$data = [Array]::CreateInstance($cellType, 256)
$occupied = [Activator]::CreateInstance($cellType)
$occupied.BatCode = 'CELL-2'
$occupied.Result = 'NG'
$occupied.Resistance = 1.25
$occupied.Voltage = 3.5
$occupied.TestTime = [datetime]'2026-09-25T12:00:00'
$data[1] = $occupied
foreach ($stage in 1..3) {
    $rows = $format.Invoke($null, @($data, $stage, [int[]]@(1, 2, 3), [string[]]@('10', '11', '12', '13')))
    if ($rows.Count -ne 1 -or $rows[0] -notmatch '^CELL-2,NG,2,1.25,3500,') {
        throw "OCV stage $stage did not save the occupied selected channel correctly."
    }
}
Write-Output 'PASS: OCV0, OCV1, and OCV2 produce occupied-channel local result rows'
