$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Find-Control([System.Windows.Forms.Control]$root, [string]$name) {
    foreach ($c in $root.Controls) {
        if ($c.Name -eq $name) { return $c }
        $r = Find-Control $c $name
        if ($r) { return $r }
    }
    return $null
}

$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
$form.MinimumSize = New-Object System.Drawing.Size(0, 0)
$applyStyle = $formType.GetMethod('ApplyFrmTestVisualStyle', [Reflection.BindingFlags] 'Instance,NonPublic')
$applyStyle.Invoke($form, @()) | Out-Null
$form.CreateControl()
$form.Size = New-Object System.Drawing.Size(1280, 902)
$form.PerformLayout()
$leftPanel = Find-Control $form 'Panel1'
$layoutField = $formType.GetField('responsiveLayout', [Reflection.BindingFlags] 'Instance,NonPublic')
$layoutField.GetValue($form).RefreshLeftContent($leftPanel)

$content = $leftPanel.Controls['LeftControlContent']

$stationButtons = @('Rbtn1','Rbtn2','Rbtn3') | ForEach-Object { $content.Controls[$_] } | Where-Object { $null -ne $_ -and $_.Checked }
if ($stationButtons.Count -ne 1) { throw "Expected exactly one selected station button, actual=$($stationButtons.Count)." }
$station = $stationButtons[0]
if ($station.Parent -ne $content) { throw 'Current station is not a direct child of LeftControlContent.' }
if ($content.Controls.GetChildIndex($station) -ne 0) { throw 'Current station is not the first left-sidebar item.' }
if ($station.Height -lt 50) { throw "Current station button was not enlarged: height=$($station.Height)." }
if ($null -ne $content.Controls['GroupBox2']) { throw 'Legacy station container must not remain in LeftControlContent.' }

$names = @($station.Name,'grpTestMode','GroupBox1','grpChannelSelection','GroupBox3','GroupBox5','GroupBox8','GroupBox7')
$boxes = @{}
foreach ($n in $names) {
    $c = $content.Controls[$n]
    if ($null -eq $c) { throw "Current left layout is missing $n." }
    $boxes[$n] = @{ Left=$c.Left; Top=$c.Top; Right=($c.Left+$c.Width); Bottom=($c.Top+$c.Height); W=$c.Width; H=$c.Height }
}

# All visible left cards must have a usable width.
foreach ($n in $names) {
    if ($boxes[$n].W -lt 300) { throw "Left card $n is collapsed: $($boxes[$n].W)x$($boxes[$n].H)" }
}

# No two visible groupboxes may overlap
for ($i=0; $i -lt $names.Length; $i++) {
    for ($j=$i+1; $j -lt $names.Length; $j++) {
        $a = $boxes[$names[$i]]; $b = $boxes[$names[$j]]
        $overlap = ($a.Left -lt $b.Right) -and ($a.Right -gt $b.Left) -and ($a.Top -lt $b.Bottom) -and ($a.Bottom -gt $b.Top)
        if ($overlap) { throw "Overlap: $($names[$i]) $($a.Left),$($a.Top) $($a.W)x$($a.H) vs $($names[$j]) $($b.Left),$($b.Top) $($b.W)x$($b.H)" }
    }
}

$leftPanel = Find-Control $form 'Panel1'
if ($content.Height -le 0 -or $content.Width -le 0) { throw "Left content has invalid bounds: content=$($content.Width)x$($content.Height)" }
$reset = $content.Controls['ResetAlarmLayout']
if ($null -eq $reset) { throw 'Alarm reset layout is missing.' }
if ($reset.Bottom -gt $content.Height) { throw "Alarm reset is outside content: bottom=$($reset.Bottom), content=$($content.Height)." }
if (($content.Height - $reset.Bottom) -gt 8) { throw "Left content has trailing blank space: content=$($content.Height), resetBottom=$($reset.Bottom)." }
if ($leftPanel.HorizontalScroll.Visible) { throw 'The left sidebar must not require horizontal scrolling.' }

Write-Output ("PASS: left panel cards are non-overlapping and scrollable; content={0}x{1}; panel={2}x{3}" -f $content.Width,$content.Height,$leftPanel.ClientSize.Width,$leftPanel.ClientSize.Height)
$form.Dispose()
