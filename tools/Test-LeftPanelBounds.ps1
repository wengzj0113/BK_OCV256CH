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
$content = $leftPanel.Controls['LeftControlContent']

$names = @('GroupBox2','GroupBox1','GroupBox6','GroupBox3','GroupBox5','GroupBox8','GroupBox4','GroupBox7')
$boxes = @{}
foreach ($n in $names) {
    $c = $content.Controls[$n]
    $boxes[$n] = @{ Left=$c.Left; Top=$c.Top; Right=($c.Left+$c.Width); Bottom=($c.Top+$c.Height); W=$c.Width; H=$c.Height }
}

# GroupBox4 must be full-size and non-collapsed
$gb4 = $boxes['GroupBox4']
if ($gb4.W -lt 350) { throw "GroupBox4 托盘控制 still collapsed: $($gb4.W)x$($gb4.H)" }

# No two visible groupboxes may overlap
for ($i=0; $i -lt $names.Length; $i++) {
    for ($j=$i+1; $j -lt $names.Length; $j++) {
        $a = $boxes[$names[$i]]; $b = $boxes[$names[$j]]
        $overlap = ($a.Left -lt $b.Right) -and ($a.Right -gt $b.Left) -and ($a.Top -lt $b.Bottom) -and ($a.Bottom -gt $b.Top)
        if ($overlap) { throw "Overlap: $($names[$i]) $($a.Left),$($a.Top) $($a.W)x$($a.H) vs $($names[$j]) $($b.Left),$($b.Top) $($b.W)x$($b.H)" }
    }
}

# probeProgress must not overlap GroupBox8
$pp = $content.Controls['probeProgress']
$gb8 = $boxes['GroupBox8']
if ($pp.Top -lt $gb8.Bottom -and $pp.Bottom -gt $gb8.Top) {
    throw "probeProgress overlaps GroupBox8: pp=$($pp.Top)..$($pp.Bottom), gb8=$($gb8.Top)..$($gb8.Bottom)"
}

# GroupBox4's txtInfo must fit inside GroupBox4
$gb4c = $content.Controls['GroupBox4']
$info = $gb4c.Controls['txtInfo']
if ($info.Bottom -gt $gb4c.Height) { throw "txtInfo bottom $($info.Bottom) exceeds GroupBox4 height $($gb4c.Height)" }

Write-Output ("PASS: left panel boxes non-overlapping; GroupBox4={0}x{1}; probeProgress at y={2} below GroupBox8 bottom={3}; txtInfo bottom={4}<=gb4 height={5}" -f $gb4.W,$gb4.H,$pp.Top,$gb8.Bottom,$info.Bottom,$gb4c.Height)
$form.Dispose()
