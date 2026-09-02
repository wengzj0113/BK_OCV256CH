$ErrorActionPreference = 'Stop'
$designer = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.Designer.vb')

foreach ($token in @('Panel1.AutoScroll = True', 'leftContentPanel.AutoSize = True', 'leftContentPanel.Dock = System.Windows.Forms.DockStyle.Top', 'rootLayout.ColumnStyles.Add', 'rightLayout.RowStyles.Add', 'Me.Controls.Add(Me.rootLayout)')) {
    if ($designer -notmatch [regex]::Escape($token)) { throw "Missing serialized layout setting: $token" }
}
if ($designer -match 'BuildDesignerLayout|BuildDesignerTestModeLayout|BuildDesignerControlBoardLayout') { throw 'Designer layout must not be reconstructed by runtime helper calls.' }
if ($designer -match 'leftContentPanel\.AutoSize\s*=\s*False') { throw 'Serialized left content must remain AutoSize so the sidebar can scroll vertically.' }
if ($designer -notmatch 'leftContentPanel\.ColumnStyles\.Add\(New System\.Windows\.Forms\.ColumnStyle\(System\.Windows\.Forms\.SizeType\.Absolute, 338\.0!\)\)') { throw 'Serialized sidebar width column is missing.' }
Write-Output 'PASS: serialized responsive content has a fixed sidebar column, scrollable content and no runtime reconstruction.'
