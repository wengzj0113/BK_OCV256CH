param([string]$AssemblyPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$exePath = if ([string]::IsNullOrWhiteSpace($AssemblyPath)) { Join-Path $root 'bin\AnyCPU\XL_OCV_256CH.exe' } else { $AssemblyPath }
$assembly = [Reflection.Assembly]::LoadFrom($exePath)
$formType = $assembly.GetType('software.frmInit', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $tabControl = $form.Controls | Where-Object { $_.Name -eq 'TabControl1' } | Select-Object -First 1
    if ($null -eq $tabControl) { throw 'TabControl1 未创建' }
    $colorPage = $tabControl.TabPages | Where-Object { $_.Name -eq 'TabPageChannelColors' } | Select-Object -First 1
    if ($null -eq $colorPage) { throw 'TabPageChannelColors 未创建' }
    $required = @('btnPassColor', 'btnTestingColor', 'btnIdleColor', 'btnResistanceNgColor', 'btnVoltageNgColor', 'btnSelectedColor', 'btnResetChannelColors')
    foreach ($name in $required) {
        $control = $null
        foreach ($child in $colorPage.Controls) {
            if ($child.Name -eq 'GroupBoxChannelColors') {
                $control = $child.Controls | Where-Object { $_.Name -eq $name } | Select-Object -First 1
                break
            }
        }
        if ($null -eq $control) { throw "$name 未在颜色页控件树中创建" }
    }
    $dialogField = $formType.GetField('_colorDialogChannel', [Reflection.BindingFlags] 'Instance,NonPublic')
    if ($null -eq $dialogField -or $null -eq $dialogField.GetValue($form)) { throw 'colorDialogChannel 未创建' }
    Write-Output 'frmInit channel color runtime tree: PASS'
}
finally {
    $form.Dispose()
}
