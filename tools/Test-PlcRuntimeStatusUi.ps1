param([string]$AssemblyPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
if (-not $AssemblyPath) { $AssemblyPath = Join-Path (Split-Path -Parent $PSScriptRoot) 'bin\build-verify-plc-timeout\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom($AssemblyPath)
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$mdiType = $assembly.GetType('software.MDIForm', $true)
$mdi = [Activator]::CreateInstance($mdiType)
$form = $null
try {
    $statusField = $mdiType.GetField('plcRuntimeStatus', $flags)
    $mdi.UpdatePlcRuntimeStatus('响应异常')
    $result = [Activator]::CreateInstance($assembly.GetType('software.StartupDeviceCheckResult', $true))
    $result.Scm = 1
    $result.Dmm34461 = 0
    $result.Resistance3561 = 1
    $result.Plc = 1
    $result.Mes = 1
    $mdi.ApplyStartupDeviceCheckResult($result, $false)
    if ($statusField.GetValue($mdi) -ne '响应异常') { throw 'Delayed startup checks must not overwrite the live PLC response status.' }

    $moduleType = $assembly.GetType('software.Module1', $true)
    $moduleType.GetField('frmMDI', [Reflection.BindingFlags]'Static,Public,NonPublic').SetValue($null, $mdi)
    $formType = $assembly.GetType('software.frmTest', $true)
    $form = [Activator]::CreateInstance($formType)
    $null = $form.Handle
    $createReporter = $formType.GetMethod('CreatePlcRuntimeStatusReporter', $flags)
    if (-not $createReporter) { throw 'PLC status callbacks must carry their connection generation.' }
    $oldReporter = $createReporter.Invoke($form, @())
    $newReporter = $createReporter.Invoke($form, @())
    $newReporter.Invoke('通信正常')
    $oldReporter.Invoke('响应异常')
    if ($statusField.GetValue($mdi) -ne '通信正常') { throw 'A callback from an old connection must not overwrite the new connection.' }

    Add-Type -TypeDefinition @'
public static class PlcStatusCallbackTest {
    public static void Queue(System.Action<string> callback) {
        var task = System.Threading.Tasks.Task.Run(() => callback("响应异常"));
        if (!task.Wait(5000)) throw new System.Exception("Status callback did not enqueue.");
    }
}
'@
    [PlcStatusCallbackTest]::Queue($newReporter)
    $formType.GetMethod('StopPlcTestCoordinator', $flags).Invoke($form, @())
    [System.Windows.Forms.Application]::DoEvents()
    if ($statusField.GetValue($mdi) -ne '未连接') { throw 'A queued callback must not restore an active status after stopping the connection.' }
    Write-Output 'PASS: live PLC status survives startup checks and rejects stale connection callbacks.'
}
finally {
    if ($form) {
        $form.PrepareForShutdown()
        $form.Dispose()
    }
    $mdi.Dispose()
}
