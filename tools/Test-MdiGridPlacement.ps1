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
$t = $assembly.GetType('software.MDIForm', $true)
$form = [Activator]::CreateInstance($t)
$form.StartPosition = 'Manual'
$form.Location = New-Object System.Drawing.Point(-5000, -5000)
$form.Size = New-Object System.Drawing.Size(1920, 1080)

$form.Show()
try {
    # frmTest Load blocks on socket/db timeouts; allow up to 30s to appear
    $deadline = [System.DateTime]::Now.AddSeconds(35)
    $frmTest = $null
    while ([System.DateTime]::Now -lt $deadline) {
        [System.Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 100
        foreach ($ch in $form.MdiChildren) { if ($ch.GetType().Name -eq 'frmTest') { $frmTest = $ch; break } }
        if ($frmTest -and $frmTest.ClientSize.Height -gt 0) { break }
    }
    if (-not $frmTest) { throw 'frmTest was not created within 30s.' }

    [System.Windows.Forms.Application]::DoEvents()
    $menu = $null; $mdi = $null; $status = $null
    foreach ($c in $form.Controls) {
        if ($c.Name -eq 'MenuStrip') { $menu = $c }
        if ($c -is [System.Windows.Forms.MdiClient]) { $mdi = $c }
        if ($c.GetType().Name -eq 'BottomStatusControl') { $status = $c }
    }
    if (-not $mdi -or -not $status) { throw 'mdiClient or BottomStatusControl not found.' }

    if ($mdi.Top -lt $menu.Bottom) { throw 'MDI client top is still covered by the menu bar.' }
    if ($mdi.Bottom -gt $status.Top) { throw 'MDI client bottom crosses the status bar top.' }

    if ($frmTest.ClientSize.Height -gt ($mdi.ClientSize.Height + 2)) {
        throw 'frmTest client height exceeds the mdiClient, so the matrix bottom would be clipped.'
    }

    $grid = Find-Control $frmTest 'channelGrid'
    if (-not $grid) { throw 'channelGrid not found.' }
    $gridSurface = $null
    $pending = New-Object System.Collections.Queue
    foreach ($child in $grid.Controls) { $pending.Enqueue($child) }
    while ($pending.Count -gt 0 -and $null -eq $gridSurface) {
        $candidate = $pending.Dequeue()
        if ($candidate.Name -eq 'gridSurface') { $gridSurface = $candidate; break }
        foreach ($nested in $candidate.Controls) { $pending.Enqueue($nested) }
    }
    if (-not $gridSurface) { throw 'Virtual channel surface not found.' }
    if ($gridSurface -isnot [System.Windows.Forms.Panel]) { throw 'Virtual channel surface must be a framework Panel.' }
    $lastRect = $gridSurface.RectangleToScreen($gridSurface.ClientRectangle)
    $statusTopScreen = $status.PointToScreen([System.Drawing.Point]::Empty).Y
    if ($lastRect.Bottom -gt ($statusTopScreen + 1)) {
        throw ('virtual channel surface bottom exceeds the status bar top; grid=({0},{1},{2},{3}), statusTop={4}, mdi=({5},{6},{7},{8}), frmTestClient={9}x{10}.' -f $lastRect.X, $lastRect.Y, $lastRect.Width, $lastRect.Height, $statusTopScreen, $mdi.Left, $mdi.Top, $mdi.Width, $mdi.Height, $frmTest.ClientSize.Width, $frmTest.ClientSize.Height)
    }

    $statusBottom = $status.Top + $status.Height
    Write-Output ('PASS: menu=[0..' + $menu.Bottom + '] mdi=[' + $mdi.Top + '..' + $mdi.Bottom + '] status=[' + $status.Top + '..' + $statusBottom + ']; frmTest.client=' + $frmTest.ClientSize.Width + 'x' + $frmTest.ClientSize.Height + '; grid bottom=' + $lastRect.Bottom + ' <= status top=' + $statusTopScreen)
}
finally {
    $form.Close()
    $form.Dispose()
}
