$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
function Find-Control([System.Windows.Forms.Control]$root, [string]$name) {
    foreach ($control in $root.Controls) {
        if ($control.Name -eq $name) { return $control }
        $nested = Find-Control $control $name
        if ($null -ne $nested) { return $nested }
    }
    return $null
}
try {
    $configure = $formType.GetMethod('ConfigureStationControls', [Reflection.BindingFlags] 'Instance,NonPublic')
    $configure.Invoke($form, @()) | Out-Null
    $form.CreateControl()
    $panel = Find-Control $form 'Panel1'
    $content = Find-Control $form 'leftContentPanel'
    if ($null -eq $panel -or $null -eq $content -or -not $panel.AutoScroll) { throw 'Serialized left panel/content is missing or not scrollable.' }
    $names = @('currentStationButton', 'grpTestMode', 'GroupBox1', 'grpChannelSelection', 'GroupBox3', 'GroupBox5', 'GroupBox8', 'GroupBox7', 'resetAlarmLayout')
    $controls = @{}
    foreach ($name in $names) {
        $control = $content.Controls[$name]
        if ($null -eq $control -or $control.Width -lt 300 -or $control.Height -le 0) { throw "Invalid left panel item: $name." }
        $controls[$name] = $control
    }
    for ($i = 0; $i -lt $names.Count; $i++) {
        for ($j = $i + 1; $j -lt $names.Count; $j++) {
            $a = $controls[$names[$i]]
            $b = $controls[$names[$j]]
            if (($a.Left -lt $b.Right) -and ($a.Right -gt $b.Left) -and ($a.Top -lt $b.Bottom) -and ($a.Bottom -gt $b.Top)) { throw "Left panel items overlap: $($names[$i])/$($names[$j])." }
        }
    }
    $reset = $controls['resetAlarmLayout']
    if ($reset.Bottom -gt $content.Height -or ($content.Height - $reset.Bottom) -gt 8) { throw "Left content has invalid trailing/reset bounds: content=$($content.Width)x$($content.Height), reset=($($reset.Left),$($reset.Top),$($reset.Width),$($reset.Height)), bottom=$($reset.Bottom)." }
    if ($panel.HorizontalScroll.Visible) { throw 'Left panel must not require horizontal scrolling.' }
    Write-Output "PASS: left panel cards are non-overlapping and scrollable; content=$($content.Width)x$($content.Height); panel=$($panel.ClientSize.Width)x$($panel.ClientSize.Height)"
}
finally { $form.Dispose() }
