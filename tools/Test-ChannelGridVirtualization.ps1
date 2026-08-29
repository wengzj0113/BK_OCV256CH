$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')

if ($source -notmatch 'Private NotInheritable Class ChannelGridSurface') {
    throw 'ChannelGridControl must use one custom drawing surface for the 256 channels.'
}
if ($source -match 'Private ReadOnly tileControls') {
    throw 'ChannelGridControl must not create and retain 256 child tile controls.'
}
if ($source -match 'BuildTiles\(\)') {
    throw 'ChannelGridControl must not build 256 WinForms tile controls during startup.'
}
if ($source -notmatch 'Private ReadOnly displayDataByChannel') {
    throw 'ChannelGridControl must retain channel display data without child controls.'
}

Write-Output 'PASS: channel overview is virtualized into one drawing surface.'
