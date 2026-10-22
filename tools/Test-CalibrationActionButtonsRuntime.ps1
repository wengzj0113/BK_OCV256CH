param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe'),
    [string]$PreviewPath = ''
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$formType = $assembly.GetType('software.FrmChannelCalibration', $true)
$form = [Activator]::CreateInstance($formType)
$flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
try {
    $panel = $formType.GetProperty('topPanel', $flags).GetValue($form)
    $null = $form.Handle
    $null = $panel.Handle
    foreach ($name in @('btnExport', 'btnRetryNg')) {
        $button = $formType.GetProperty($name, $flags).GetValue($form)
        $null = $button.Handle
        if ($button.Parent -ne $panel) {
            throw "$name must belong to topPanel so the docked panel cannot cover it."
        }
    }
    foreach ($width in @(1544, 1920)) {
        $form.ClientSize = New-Object System.Drawing.Size($width, 800)
        $form.PerformLayout()
        $panel.PerformLayout()
        foreach ($name in @('btnExport', 'btnRetryNg')) {
            $button = $formType.GetProperty($name, $flags).GetValue($form)
            if (-not $panel.ClientRectangle.Contains($button.Bounds)) {
                throw "$name is clipped at width $width."
            }
            $center = New-Object System.Drawing.Point(($button.Left + [int]($button.Width / 2)), ($button.Top + [int]($button.Height / 2)))
            if ($panel.GetChildAtPoint($center, [System.Windows.Forms.GetChildAtPointSkip]::None) -ne $button) {
                throw "$name is covered at width $width."
            }
        }
    }
    if ($PreviewPath) {
        function Initialize-ChildHandles($control) {
            $null = $control.Handle
            foreach ($child in $control.Controls) { Initialize-ChildHandles $child }
        }
        Initialize-ChildHandles $form
        $bitmap = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
        try {
            $form.DrawToBitmap($bitmap, $form.ClientRectangle)
            $bitmap.Save($PreviewPath, [System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose() }
    }
} finally { $form.Dispose() }
Write-Output 'PASS: export and NG retest buttons belong to the toolbar, fit and remain uncovered at both widths.'
