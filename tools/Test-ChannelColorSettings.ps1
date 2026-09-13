$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Assert-Contains([string]$path, [string]$pattern, [string]$message) {
    $fullPath = Join-Path $root $path
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "$message ($path 文件不存在)"
    }
    $text = Get-Content -LiteralPath $fullPath -Raw
    if ($text -notmatch [regex]::Escape($pattern)) {
        throw "$message ($path 缺少: $pattern)"
    }
}

Assert-Contains 'Module1.vb' 'ChannelPassColorArgb' 'Initset 未追加 PASS 颜色字段'
Assert-Contains 'frmInit.Designer.vb' 'TabPageChannelColors' 'frmInit 未增加独立颜色页'
Assert-Contains 'ChannelStatusColors.vb' 'Public Enum ChannelStatusKind' '缺少通道状态枚举'
Assert-Contains 'ChannelStatusColors.vb' 'Public Shared Function ForStatus' '缺少统一状态取色入口'
Assert-Contains 'ChannelStatusColors.vb' 'Color.FromArgb(255' '缺少不透明颜色规范化'
Assert-Contains 'ChannelStatusColors.vb' 'ResistanceNg' 'UIControls 未定义内阻 NG 状态'
Assert-Contains 'ChannelStatusColors.vb' 'VoltageNgColor' 'UIControls 未定义电压 NG 颜色'
Assert-Contains 'ChannelColorSettings.vb' 'DefaultPass' '主应用缺少 PASS 默认颜色'
Assert-Contains 'ChannelColorSettings.vb' 'DefaultTesting' '主应用缺少测试中默认颜色'
Assert-Contains 'ChannelColorSettings.vb' 'DefaultIdle' '主应用缺少空闲默认颜色'
Assert-Contains 'ChannelColorSettings.vb' 'DefaultResistanceNg' '主应用缺少内阻 NG 默认颜色'
Assert-Contains 'ChannelColorSettings.vb' 'DefaultVoltageNg' '主应用缺少电压 NG 默认颜色'
Assert-Contains 'frmTest.vb' 'RESISTANCE_NG' '主页面未细分内阻 NG'
Assert-Contains 'frmTest.vb' 'VOLTAGE_NG' '主页面未细分电压 NG'
Assert-Contains 'ChannelGridControl.vb' 'Public Sub ApplyStatusColors' '主通道格未接入调色板'
Assert-Contains 'ChannelGridControl.vb' 'ChannelStatusColors' '主通道格未引用调色板'
Assert-Contains 'ChannelTileControl.vb' 'ChannelStatusColors.ForStatus' '通道控件未接入统一调色板'
Assert-Contains 'frmTest.vb' 'ChannelColorSettings.FromInitset' 'frmTest 未从配置建立调色板'
Assert-Contains 'frmInit.Designer.vb' 'Me.TabPageChannelColors = New System.Windows.Forms.TabPage()' '颜色页未在 Designer 初始化'
Assert-Contains 'frmInit.Designer.vb' 'Me.colorDialogChannel = New System.Windows.Forms.ColorDialog()' 'ColorDialog 未托管到 Designer'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnResetChannelColors' 'Designer 未声明恢复默认按钮'
Assert-Contains 'frmInit.Designer.vb' 'Me.TabControl1.Controls.Add(Me.TabPageChannelColors)' '颜色页未加入 TabControl'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnPassColor = New System.Windows.Forms.Button()' 'Designer 缺少 PASS 颜色按钮'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnTestingColor = New System.Windows.Forms.Button()' 'Designer 缺少测试中颜色按钮'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnIdleColor = New System.Windows.Forms.Button()' 'Designer 缺少空闲颜色按钮'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnResistanceNgColor = New System.Windows.Forms.Button()' 'Designer 缺少内阻 NG 颜色按钮'
Assert-Contains 'frmInit.Designer.vb' 'Me.btnVoltageNgColor = New System.Windows.Forms.Button()' 'Designer 缺少电压 NG 颜色按钮'
Assert-Contains 'frmInit.vb' 'btnResetChannelColors' 'frmInit 未接入恢复默认按钮'
Assert-Contains 'Module1.vb' 'BW.Write(Initsetting.ChannelPassColorArgb)' '未写入 PASS 颜色'
Assert-Contains 'Module1.vb' 'Initsetting.ChannelVoltageNgColorArgb = BR.ReadInt32()' '未读取电压 NG 颜色'
Assert-Contains 'Module1.vb' 'DefaultVoltageNg' '旧配置缺少颜色默认值回退'
Assert-Contains 'frmTest.vb' 'testingChannelNumbers.Contains(channelNo)' '测试中状态未保留'

Write-Output 'Channel color contract: PASS'
