$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$module = $assembly.GetType('software.Module1', $true)
$resultsField = $module.GetField('RecieveData')
$results = $resultsField.GetValue($null)
$first = $results.GetValue(0)
$first.GetType().GetField('BatCode').SetValue($first, 'CELL-1')
$results.SetValue($first, 0)
$uploadType = $assembly.GetType('software.ResultDataUploadAssembly', $true)
$upload = [Activator]::CreateInstance($uploadType, @([System.Windows.Forms.ListBox]::new()))
$upload.ChannelNumbers = [System.Collections.Generic.List[int]]::new([int[]]@(1))
$upload.CaptureSnapshot()
$first.GetType().GetField('BatCode').SetValue($first, 'NEXT-TRAY')
$results.SetValue($first, 0)
$getOutList = $uploadType.GetMethod('GetOutList', [Reflection.BindingFlags]'Instance,NonPublic')
$rows = $getOutList.Invoke($upload, @())
if ($rows[0]['Out_Lot_No'] -ne 'CELL-1') { throw 'MES upload used a later tray instead of its captured result.' }
Write-Output 'PASS: MES upload uses captured cycle data'
