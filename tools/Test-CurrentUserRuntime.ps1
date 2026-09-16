param(
    [string]$AssemblyPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'bin\AnyCPU\XL_OCV_256CH.exe')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

if (-not (Test-Path -LiteralPath $AssemblyPath)) {
    throw "Assembly not found: $AssemblyPath"
}

$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$mdiType = $assembly.GetType('software.MDIForm', $true)
$form = [Activator]::CreateInstance($mdiType)

try {
    $updateMethod = $mdiType.GetMethod('UpdateBottomStatus', [Reflection.BindingFlags] 'Instance, NonPublic')
    if ($null -eq $updateMethod) { throw 'MDIForm.UpdateBottomStatus was not found.' }
    $updateMethod.Invoke($form, $null)

    $statusField = $mdiType.GetField('bottomStatusControl', [Reflection.BindingFlags] 'Instance, Public, NonPublic')
    if ($null -ne $statusField) {
        $statusControl = $statusField.GetValue($form)
    } else {
        $statusControl = $form.Controls | Where-Object { $_.GetType().Name -eq 'BottomStatusControl' } | Select-Object -First 1
    }
    if ($null -eq $statusControl) { throw 'MDIForm bottom status control was not found.' }

    $userField = $statusControl.GetType().GetField('userLabel', [Reflection.BindingFlags] 'Instance, Public, NonPublic')
    if ($null -eq $userField) { throw 'BottomStatusControl.userLabel was not found.' }
    $userLabel = $userField.GetValue($statusControl)
    $expected = ([char]0x25CF).ToString() + ' ' +
                ([char]0x5F53).ToString() + ([char]0x524D).ToString() +
                ([char]0x7528).ToString() + ([char]0x6237).ToString() +
                ([char]0xFF1A).ToString() +
                ([char]0x672A).ToString() + ([char]0x767B).ToString() + ([char]0x5F55).ToString()
    if ($userLabel.Text -ne $expected) {
        throw "Unexpected current-user status: '$($userLabel.Text)' (expected '$expected')."
    }

    if ($form.operTypeNum -ne 0) {
        throw "Unexpected startup operator type: $($form.operTypeNum) (expected 0)."
    }

    Write-Output 'Current-user runtime contract: PASS'
}
finally {
    $form.Dispose()
}
