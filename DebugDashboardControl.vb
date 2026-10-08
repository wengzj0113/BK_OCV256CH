Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

''' <summary>
''' 调试中心的纯 UI 控件。设备和业务操作通过事件交给 frmDebug 适配。
''' </summary>
Public Class DebugDashboardControl
    Inherits UserControl

    Private ReadOnly deviceRows As New Dictionary(Of DebugDeviceId, DebugDeviceRow)()
    Private ReadOnly logEntries As New List(Of DebugLogEntry)()
    Private ReadOnly deviceList As New FlowLayoutPanel()
    Private ReadOnly logText As New RichTextBox()
    Private ReadOnly communicationText As New RichTextBox()
    Private ReadOnly payloadText As New TextBox()
    Private ReadOnly interfaceCombo As New ComboBox()
    Private ReadOnly formatCombo As New ComboBox()
    Private ReadOnly sendButton As New Button()
    Private ReadOnly manualPanel As New Panel()
    Private ReadOnly connectedSummary As New Label()
    Private ReadOnly failedSummary As New Label()
    Private ReadOnly pendingSummary As New Label()
    Private ReadOnly runStateLabel As New Label()
    Private ReadOnly allProbeButton As New Button()
    Private ReadOnly channelSelector As New NumericUpDown()
    Private ReadOnly copyLogButton As New Button()
    Private ReadOnly clearLogButton As New Button()
    Private ReadOnly exportLogButton As New Button()
    Private ReadOnly closeButton As New Button()
    Private dashboardDisposed As Boolean

    Public Event DeviceTestRequested As EventHandler(Of DebugDeviceEventArgs)
    Public Event AllDevicesTestRequested As EventHandler
    Public Event CommunicationSendRequested As EventHandler(Of DebugCommunicationRequestEventArgs)
    Public Event ManualActionRequested As EventHandler(Of DebugManualActionEventArgs)
    Public Event LogClearRequested As EventHandler
    Public Event CloseRequested As EventHandler

    Public Sub New()
        DoubleBuffered = True
        Dock = DockStyle.Fill
        BackColor = UiTheme.PageBackground
        Font = UiTheme.UiFont
        BuildLayout()
        BuildDeviceRows()
        SetRunningState(False)
    End Sub

    Public Sub SetDeviceTesting(deviceId As DebugDeviceId, testing As Boolean)
        If dashboardDisposed OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of DebugDeviceId, Boolean)(AddressOf SetDeviceTesting), deviceId, testing)
            Return
        End If

        Dim row As DebugDeviceRow = Nothing
        If deviceRows.TryGetValue(deviceId, row) Then row.SetTesting(testing)
    End Sub

    Public Sub SetDeviceResult(result As DebugDeviceResult)
        If result Is Nothing OrElse dashboardDisposed OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of DebugDeviceResult)(AddressOf SetDeviceResult), result)
            Return
        End If

        Dim row As DebugDeviceRow = Nothing
        If deviceRows.TryGetValue(result.DeviceId, row) Then row.SetResult(result)
        AddLogInternal(GetLevel(result.State), DebugProbeService.GetDeviceName(result.DeviceId) & "：" & result.Message)
        UpdateSummary()
    End Sub

    Public Sub SetDeviceResults(results As IEnumerable(Of DebugDeviceResult))
        If results Is Nothing Then Return
        For Each result As DebugDeviceResult In results
            SetDeviceResult(result)
        Next
    End Sub

    Public Sub AppendLog(level As String, message As String)
        If dashboardDisposed OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of String, String)(AddressOf AppendLog), level, message)
            Return
        End If
        AddLogInternal(level, message)
    End Sub

    Public Sub AppendCommunicationExchange(success As Boolean, response As Byte(), message As String)
        If dashboardDisposed OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of Boolean, Byte(), String)(AddressOf AppendCommunicationExchange), success, response, message)
            Return
        End If

        Dim prefix As String = If(success, "RX", "ERR")
        Dim body As String = If(response Is Nothing OrElse response.Length = 0, message, FormatBytes(response, CurrentDisplayMode()))
        AppendCommunicationLine(prefix, body)
        AddLogInternal(If(success, "INFO", "ERROR"), "通信控制台：" & message)
    End Sub

    Public Sub SetRunningState(running As Boolean)
        If dashboardDisposed OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of Boolean)(AddressOf SetRunningState), running)
            Return
        End If

        runStateLabel.Text = If(running, "正式测试运行中 · 调试操作已锁定", "正式测试未运行 · 可进行调试")
        runStateLabel.ForeColor = If(running, UiTheme.Warning, UiTheme.Success)
        allProbeButton.Enabled = Not running
        sendButton.Enabled = Not running AndAlso interfaceCombo.SelectedIndex >= 0
        SetButtonTreeEnabled(manualPanel, Not running)
    End Sub

    Public Sub ResetAllDeviceStates()
        For Each deviceId As DebugDeviceId In [Enum].GetValues(GetType(DebugDeviceId))
            SetDeviceResult(DebugDeviceResult.PendingResult(deviceId, DebugProbeService.GetInterfaceDescription(deviceId)))
        Next
        UpdateSummary()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        dashboardDisposed = True
        MyBase.Dispose(disposing)
    End Sub

    Private Sub BuildLayout()
        Dim outer As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = UiTheme.PageBackground,
            .Padding = New Padding(14, 12, 14, 12)
        }
        outer.RowStyles.Add(New RowStyle(SizeType.Absolute, 58.0F))
        outer.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        outer.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
        Controls.Add(outer)

        outer.Controls.Add(BuildHeader(), 0, 0)
        outer.Controls.Add(BuildTabs(), 0, 1)
        outer.Controls.Add(BuildFooter(), 0, 2)
    End Sub

    Private Function BuildHeader() As Control
        Dim header As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.HeaderBackground, .Padding = New Padding(16, 8, 16, 8)}
        Dim title As New Label() With {.Text = "设备调试中心", .ForeColor = Color.White, .Font = UiTheme.UiFontTitle, .AutoSize = True, .Location = New Point(14, 8)}
        Dim subtitle As New Label() With {.Text = "XL-OCV-256CH · 连接诊断 / 通信收发 / 手动控制", .ForeColor = Color.FromArgb(205, 220, 238), .AutoSize = True, .Location = New Point(16, 35)}
        header.Controls.Add(title)
        header.Controls.Add(subtitle)
        Return header
    End Function

    Private Function BuildTabs() As Control
        Dim tabs As New TabControl() With {.Dock = DockStyle.Fill, .Padding = New Point(14, 5)}
        tabs.TabPages.Add(BuildOverviewPage())
        tabs.TabPages.Add(BuildCommunicationPage())
        tabs.TabPages.Add(BuildManualPage())
        tabs.TabPages.Add(BuildLogPage())
        Return tabs
    End Function

    Private Function BuildOverviewPage() As TabPage
        Dim page As New TabPage("设备总览") With {.Name = "TabDeviceOverview", .BackColor = UiTheme.PageBackground, .Padding = New Padding(10)}
        Dim layout As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 74.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        page.Controls.Add(layout)

        layout.Controls.Add(BuildSummaryPanel(), 0, 0)
        layout.Controls.Add(BuildOverviewToolbar(), 0, 1)
        deviceList.FlowDirection = FlowDirection.TopDown
        deviceList.WrapContents = False
        deviceList.AutoScroll = True
        deviceList.Dock = DockStyle.Fill
        deviceList.BackColor = UiTheme.PageBackground
        deviceList.Padding = New Padding(0, 3, 4, 3)
        AddHandler deviceList.Resize, AddressOf DeviceList_Resize
        layout.Controls.Add(deviceList, 0, 2)
        Return page
    End Function

    Private Function BuildSummaryPanel() As Control
        Dim summary As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 4, .RowCount = 1, .BackColor = UiTheme.PageBackground}
        summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        summary.Controls.Add(BuildMetricCard("在线 / 已管理", connectedSummary, UiTheme.Success), 0, 0)
        summary.Controls.Add(BuildMetricCard("失败", failedSummary, UiTheme.Danger), 1, 0)
        summary.Controls.Add(BuildMetricCard("待检查 / 不可用", pendingSummary, UiTheme.Warning), 2, 0)
        Dim stateCard As Panel = BuildMetricCard("运行保护", runStateLabel, UiTheme.Primary)
        summary.Controls.Add(stateCard, 3, 0)
        Return summary
    End Function

    Private Function BuildMetricCard(caption As String, valueLabel As Label, accent As Color) As Panel
        Dim card As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.CardBackground, .Margin = New Padding(0, 0, 8, 0), .Padding = New Padding(12, 7, 8, 5), .BorderStyle = BorderStyle.FixedSingle}
        Dim captionLabel As New Label() With {.Text = caption, .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Font = UiTheme.UiFont, .Location = New Point(11, 7)}
        valueLabel.AutoSize = True
        valueLabel.Text = "0"
        valueLabel.ForeColor = accent
        valueLabel.Font = UiTheme.UiFontBold
        valueLabel.Location = New Point(11, 28)
        card.Controls.Add(captionLabel)
        card.Controls.Add(valueLabel)
        Return card
    End Function

    Private Function BuildOverviewToolbar() As Control
        Dim bar As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.PageBackground}
        allProbeButton.Text = "一键检查全部"
        allProbeButton.Size = New Size(122, 30)
        allProbeButton.Location = New Point(0, 4)
        UiTheme.ApplyButtonStyle(allProbeButton, UiTheme.ButtonRole.Primary)
        AddHandler allProbeButton.Click, Sub(sender, args) RaiseEvent AllDevicesTestRequested(Me, EventArgs.Empty)
        bar.Controls.Add(allProbeButton)
        Dim hint As New Label() With {.Text = "单卡检查不会启动正式测试；结果和耗时会记录到运行日志。", .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Location = New Point(136, 12)}
        bar.Controls.Add(hint)
        Return bar
    End Function

    Private Sub BuildDeviceRows()
        Dim deviceIds() As DebugDeviceId = {
            DebugDeviceId.ControlBoard,
            DebugDeviceId.MiddleMachine,
            DebugDeviceId.Hioki3562,
            DebugDeviceId.Keysight34461,
            DebugDeviceId.PlcTray,
            DebugDeviceId.IoCard,
            DebugDeviceId.Scanner,
            DebugDeviceId.Retest3562,
            DebugDeviceId.Database,
            DebugDeviceId.Mes
        }
        For Each deviceId As DebugDeviceId In deviceIds
            Dim row As New DebugDeviceRow(deviceId)
            row.SetResult(DebugDeviceResult.PendingResult(deviceId, DebugProbeService.GetInterfaceDescription(deviceId)))
            Dim currentId As DebugDeviceId = deviceId
            AddHandler row.TestButton.Click, Sub(sender, args) RaiseEvent DeviceTestRequested(Me, New DebugDeviceEventArgs(currentId))
            deviceRows.Add(deviceId, row)
            deviceList.Controls.Add(row.Root)
        Next
        DeviceList_Resize(deviceList, EventArgs.Empty)
        UpdateSummary()
    End Sub

    Private Sub DeviceList_Resize(sender As Object, e As EventArgs)
        Dim width As Integer = Math.Max(440, deviceList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8)
        For Each row As DebugDeviceRow In deviceRows.Values
            row.Root.Width = width
        Next
    End Sub

    Private Function BuildCommunicationPage() As TabPage
        Dim page As New TabPage("通信控制台") With {.Name = "TabCommunicationConsole", .BackColor = UiTheme.PageBackground, .Padding = New Padding(10)}
        Dim layout As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 4}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 92.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        page.Controls.Add(layout)

        Dim selectBar As New FlowLayoutPanel() With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .WrapContents = False, .Padding = New Padding(0, 5, 0, 0), .BackColor = UiTheme.PageBackground}
        selectBar.Controls.Add(New Label() With {.Text = "接口", .AutoSize = True, .Margin = New Padding(0, 7, 6, 0), .ForeColor = UiTheme.TextPrimary})
        interfaceCombo.DropDownStyle = ComboBoxStyle.DropDownList
        interfaceCombo.Width = 190
        interfaceCombo.Items.AddRange(New Object() {"SCM / 控制板", "中位机 / RS485", "日置 3562"})
        interfaceCombo.SelectedIndex = 0
        AddHandler interfaceCombo.SelectedIndexChanged, AddressOf InterfaceCombo_SelectedIndexChanged
        selectBar.Controls.Add(interfaceCombo)
        selectBar.Controls.Add(New Label() With {.Text = "显示格式", .AutoSize = True, .Margin = New Padding(16, 7, 6, 0), .ForeColor = UiTheme.TextPrimary})
        formatCombo.DropDownStyle = ComboBoxStyle.DropDownList
        formatCombo.Width = 100
        formatCombo.Items.AddRange(New Object() {"Hex", "ASCII"})
        formatCombo.SelectedIndex = 0
        selectBar.Controls.Add(formatCombo)
        layout.Controls.Add(selectBar, 0, 0)

        Dim payloadPanel As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.CardBackground, .Padding = New Padding(10), .BorderStyle = BorderStyle.FixedSingle}
        payloadText.Multiline = True
        payloadText.ScrollBars = ScrollBars.Vertical
        payloadText.Dock = DockStyle.Fill
        payloadText.Text = "AA 03 00 00"
        UiTheme.ApplyTextBoxStyle(payloadText)
        payloadPanel.Controls.Add(payloadText)
        layout.Controls.Add(payloadPanel, 0, 1)

        Dim actionBar As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.PageBackground}
        sendButton.Text = "发送"
        sendButton.Size = New Size(90, 30)
        sendButton.Location = New Point(0, 4)
        UiTheme.ApplyButtonStyle(sendButton, UiTheme.ButtonRole.Primary)
        AddHandler sendButton.Click, AddressOf SendButton_Click
        actionBar.Controls.Add(sendButton)
        Dim clearButton As New Button() With {.Text = "清空收发", .Size = New Size(90, 30), .Location = New Point(98, 4)}
        UiTheme.ApplyButtonStyle(clearButton, UiTheme.ButtonRole.Neutral)
        AddHandler clearButton.Click, Sub(sender, args) communicationText.Clear()
        actionBar.Controls.Add(clearButton)
        actionBar.Controls.Add(New Label() With {.Text = "仅支持已打开的 SCM、RS485 和 3562 串口；不在此页启动正式测试。", .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Location = New Point(202, 12)})
        layout.Controls.Add(actionBar, 0, 2)

        communicationText.Dock = DockStyle.Fill
        communicationText.ReadOnly = True
        communicationText.BackColor = Color.FromArgb(27, 36, 49)
        communicationText.ForeColor = Color.FromArgb(220, 230, 240)
        communicationText.Font = New Font("Consolas", 9.0F)
        communicationText.BorderStyle = BorderStyle.None
        layout.Controls.Add(communicationText, 0, 3)
        Return page
    End Function

    Private Sub InterfaceCombo_SelectedIndexChanged(sender As Object, e As EventArgs)
        sendButton.Enabled = Not isRuning AndAlso interfaceCombo.SelectedIndex >= 0
    End Sub

    Private Sub SendButton_Click(sender As Object, e As EventArgs)
        If isRuning Then
            AddLogInternal("WARN", "通信控制台：正式测试运行中，已阻止发送")
            Return
        End If

        Try
            Dim payload As Byte() = ParsePayload(payloadText.Text, CurrentDisplayMode())
            If payload.Length = 0 Then Throw New FormatException("发送数据不能为空")
            AppendCommunicationLine("TX", FormatBytes(payload, CurrentDisplayMode()))
            RaiseEvent CommunicationSendRequested(Me, New DebugCommunicationRequestEventArgs(interfaceCombo.Text, payload, CurrentDisplayMode()))
        Catch ex As Exception
            AppendCommunicationExchange(False, Nothing, ex.Message)
        End Try
    End Sub

    Private Function CurrentDisplayMode() As String
        If formatCombo.SelectedIndex = 1 Then Return "ASCII"
        Return "Hex"
    End Function

    Private Shared Function ParsePayload(value As String, displayMode As String) As Byte()
        If String.Equals(displayMode, "ASCII", StringComparison.OrdinalIgnoreCase) Then
            Return Encoding.UTF8.GetBytes(If(value, String.Empty))
        End If

        Dim bytes As New List(Of Byte)()
        Dim tokens() As String = Regex.Split(If(value, String.Empty).Trim(), "[\s,;]+")
        For Each token As String In tokens
            If String.IsNullOrWhiteSpace(token) Then Continue For
            Dim normalized As String = token.Trim()
            If normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then normalized = normalized.Substring(2)
            If normalized.Length > 2 Then Throw New FormatException("Hex 数据必须以字节为单位：" & token)
            bytes.Add(Byte.Parse(normalized, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
        Next
        Return bytes.ToArray()
    End Function

    Private Shared Function FormatBytes(bytes As Byte(), displayMode As String) As String
        If bytes Is Nothing OrElse bytes.Length = 0 Then Return "<空>"
        If String.Equals(displayMode, "ASCII", StringComparison.OrdinalIgnoreCase) Then
            Return Encoding.UTF8.GetString(bytes)
        End If
        Return String.Join(" ", bytes.Select(Function(item) item.ToString("X2")))
    End Function

    Private Sub AppendCommunicationLine(prefix As String, body As String)
        communicationText.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") & "  " & prefix.PadRight(3) & " " & body & Environment.NewLine)
        communicationText.ScrollToCaret()
    End Sub

    Private Function BuildManualPage() As TabPage
        Dim page As New TabPage("手动控制") With {.Name = "TabManualControl", .BackColor = UiTheme.PageBackground, .Padding = New Padding(10)}
        Dim layout As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))
        page.Controls.Add(layout)

        Dim warning As New Label() With {.Text = "手动动作可能改变夹具或中位机状态；危险动作会二次确认，正式测试期间自动锁定。", .Dock = DockStyle.Fill, .Padding = New Padding(10, 12, 0, 0), .ForeColor = UiTheme.Warning, .BackColor = Color.FromArgb(255, 250, 240)}
        layout.Controls.Add(warning, 0, 0)
        manualPanel.Dock = DockStyle.Fill
        manualPanel.BackColor = UiTheme.PageBackground
        BuildManualButtons()
        layout.Controls.Add(manualPanel, 0, 1)
        layout.Controls.Add(New Label() With {.Text = "通道计划只下发选择，不会启动正式测量。", .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Padding = New Padding(0, 8, 0, 0)}, 0, 2)
        Return page
    End Function

    Private Sub BuildManualButtons()
        Dim grid As New TableLayoutPanel() With {.Dock = DockStyle.Top, .AutoSize = True, .ColumnCount = 3, .RowCount = 4, .BackColor = UiTheme.PageBackground, .Padding = New Padding(0, 8, 0, 0)}
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.34F))
        manualPanel.Controls.Add(grid)

        AddManualButton(grid, "读取中位机状态", "ReadMiddleMachineStatus", 0, 0, UiTheme.ButtonRole.Neutral)
        AddManualButton(grid, "中位机安全复位", "ResetMiddleMachine", 1, 0, UiTheme.ButtonRole.Warning)
        AddManualButton(grid, "停止中位机", "StopMiddleMachine", 2, 0, UiTheme.ButtonRole.Danger)

        Dim channelPanel As New FlowLayoutPanel() With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .WrapContents = False, .Padding = New Padding(4, 6, 4, 4), .BackColor = UiTheme.CardBackground, .BorderStyle = BorderStyle.FixedSingle}
        channelPanel.Controls.Add(New Label() With {.Text = "通道", .AutoSize = True, .Margin = New Padding(0, 7, 5, 0)})
        channelSelector.Minimum = 1
        channelSelector.Maximum = MAX_CHANNEL
        channelSelector.Value = 1
        channelSelector.Width = 70
        channelPanel.Controls.Add(channelSelector)
        Dim configureButton As New Button() With {.Text = "下发选择", .Size = New Size(88, 29), .Tag = "ConfigureChannel"}
        UiTheme.ApplyButtonStyle(configureButton, UiTheme.ButtonRole.Primary)
        AddHandler configureButton.Click, AddressOf ManualButton_Click
        channelPanel.Controls.Add(configureButton)
        grid.Controls.Add(channelPanel, 0, 1)
        grid.SetColumnSpan(channelPanel, 2)
        grid.Controls.Add(New Label() With {.Text = "不启动测量", .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Anchor = AnchorStyles.Left, .Margin = New Padding(10, 10, 0, 0)}, 2, 1)

        AddManualButton(grid, "读取托盘状态", "ReadTrayStatus", 0, 2, UiTheme.ButtonRole.Neutral)
        AddManualButton(grid, "开启托盘", "OpenTray", 1, 2, UiTheme.ButtonRole.Warning)
        AddManualButton(grid, "闭合托盘", "CloseTray", 2, 2, UiTheme.ButtonRole.Warning)

        AddManualButton(grid, "PLC 复位", "ResetPlc", 0, 3, UiTheme.ButtonRole.Danger)
        AddManualButton(grid, "清除当前日志", "ClearCurrentLog", 1, 3, UiTheme.ButtonRole.Neutral)
    End Sub

    Private Sub AddManualButton(grid As TableLayoutPanel, text As String, actionName As String, column As Integer, row As Integer, role As UiTheme.ButtonRole)
        Dim button As New Button() With {.Text = text, .Tag = actionName, .Dock = DockStyle.Fill, .Height = 34, .Margin = New Padding(4)}
        UiTheme.ApplyButtonStyle(button, role)
        AddHandler button.Click, AddressOf ManualButton_Click
        grid.Controls.Add(button, column, row)
    End Sub

    Private Sub ManualButton_Click(sender As Object, e As EventArgs)
        If isRuning Then
            AddLogInternal("WARN", "手动控制：正式测试运行中，已阻止操作")
            Return
        End If

        Dim button As Button = TryCast(sender, Button)
        If button Is Nothing Then Return
        Dim actionName As String = CStr(button.Tag)
        If actionName = "ClearCurrentLog" Then
            logEntries.Clear()
            logText.Clear()
            RaiseEvent LogClearRequested(Me, EventArgs.Empty)
            Return
        End If

        If actionName = "OpenTray" OrElse actionName = "CloseTray" OrElse actionName = "ResetPlc" OrElse actionName = "ConfigureChannel" Then
            Dim prompt As String = If(actionName = "ConfigureChannel", "确认下发通道选择（只配置，不启动测试）？", "确认执行手动设备动作？")
            If MessageBox.Show(prompt, "调试操作确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        End If

        Dim value As Integer = If(actionName = "ConfigureChannel", CInt(channelSelector.Value), 0)
        RaiseEvent ManualActionRequested(Me, New DebugManualActionEventArgs(actionName, value))
    End Sub

    Private Function BuildLogPage() As TabPage
        Dim page As New TabPage("运行日志") With {.Name = "TabRuntimeLog", .BackColor = UiTheme.PageBackground, .Padding = New Padding(10)}
        Dim layout As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        page.Controls.Add(layout)
        Dim toolbar As New FlowLayoutPanel() With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .WrapContents = False, .Padding = New Padding(0, 4, 0, 0)}
        copyLogButton.Text = "复制日志"
        clearLogButton.Text = "清空日志"
        exportLogButton.Text = "导出日志"
        For Each button As Button In New Button() {copyLogButton, clearLogButton, exportLogButton}
            button.Size = New Size(90, 30)
            UiTheme.ApplyButtonStyle(button, UiTheme.ButtonRole.Neutral)
            toolbar.Controls.Add(button)
        Next
        AddHandler copyLogButton.Click, AddressOf CopyLogButton_Click
        AddHandler clearLogButton.Click, AddressOf ClearLogButton_Click
        AddHandler exportLogButton.Click, AddressOf ExportLogButton_Click
        layout.Controls.Add(toolbar, 0, 0)
        logText.Dock = DockStyle.Fill
        logText.ReadOnly = True
        logText.BackColor = Color.FromArgb(27, 36, 49)
        logText.ForeColor = Color.FromArgb(220, 230, 240)
        logText.Font = New Font("Consolas", 9.0F)
        logText.BorderStyle = BorderStyle.None
        layout.Controls.Add(logText, 0, 1)
        Return page
    End Function

    Private Sub CopyLogButton_Click(sender As Object, e As EventArgs)
        If logText.TextLength > 0 Then Clipboard.SetText(logText.Text)
    End Sub

    Private Sub ClearLogButton_Click(sender As Object, e As EventArgs)
        logEntries.Clear()
        logText.Clear()
        RaiseEvent LogClearRequested(Me, EventArgs.Empty)
    End Sub

    Private Sub ExportLogButton_Click(sender As Object, e As EventArgs)
        Using dialog As New SaveFileDialog() With {.Filter = "文本文件|*.txt", .FileName = "debug_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".txt"}
            If dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            File.WriteAllText(dialog.FileName, logText.Text, Encoding.UTF8)
            AddLogInternal("INFO", "运行日志已导出：" & dialog.FileName)
        End Using
    End Sub

    Private Function BuildFooter() As Control
        Dim footer As New Panel() With {.Dock = DockStyle.Fill, .BackColor = UiTheme.CardBackground, .Padding = New Padding(6, 5, 0, 4), .BorderStyle = BorderStyle.FixedSingle}
        closeButton.Text = "关闭"
        closeButton.Size = New Size(84, 30)
        closeButton.Dock = DockStyle.Right
        UiTheme.ApplyButtonStyle(closeButton, UiTheme.ButtonRole.Neutral)
        AddHandler closeButton.Click, Sub(sender, args) RaiseEvent CloseRequested(Me, EventArgs.Empty)
        footer.Controls.Add(closeButton)
        footer.Controls.Add(New Label() With {.Text = "提示：调试动作会写入应用日志，正式测试期间共享设备自动锁定。", .AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Location = New Point(8, 10)})
        Return footer
    End Function

    Private Sub AddLogInternal(level As String, message As String)
        If dashboardDisposed OrElse IsDisposed Then Return
        Dim entry As New DebugLogEntry With {.Timestamp = DateTime.Now, .Level = If(String.IsNullOrWhiteSpace(level), "INFO", level), .Message = If(message, String.Empty)}
        logEntries.Add(entry)
        logText.AppendText(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  [" & entry.Level.PadRight(5) & "] " & entry.Message & Environment.NewLine)
        logText.ScrollToCaret()
        AppLog.Write("Debug", "[" & entry.Level & "] " & entry.Message)
    End Sub

    Private Sub UpdateSummary()
        Dim connected As Integer = 0
        Dim failed As Integer = 0
        Dim pending As Integer = 0
        For Each row As DebugDeviceRow In deviceRows.Values
            Select Case row.CurrentState
                Case DebugDeviceState.Connected, DebugDeviceState.Managed : connected += 1
                Case DebugDeviceState.Failed : failed += 1
                Case Else : pending += 1
            End Select
        Next
        connectedSummary.Text = connected.ToString()
        failedSummary.Text = failed.ToString()
        pendingSummary.Text = pending.ToString()
    End Sub

    Private Shared Function GetLevel(state As DebugDeviceState) As String
        Select Case state
            Case DebugDeviceState.Connected, DebugDeviceState.Managed : Return "INFO"
            Case DebugDeviceState.Failed : Return "ERROR"
            Case DebugDeviceState.Unavailable : Return "WARN"
            Case Else : Return "INFO"
        End Select
    End Function

    Private Shared Sub SetButtonTreeEnabled(container As Control, enabled As Boolean)
        For Each child As Control In container.Controls
            Dim button As Button = TryCast(child, Button)
            If button IsNot Nothing Then button.Enabled = enabled
            If child.HasChildren Then SetButtonTreeEnabled(child, enabled)
        Next
    End Sub

    Private NotInheritable Class DebugDeviceRow
        Public ReadOnly Root As Panel
        Public ReadOnly TestButton As Button
        Private ReadOnly statusDot As Label
        Private ReadOnly statusText As Label
        Private ReadOnly detailText As Label
        Private ReadOnly timeText As Label
        Private deviceState As DebugDeviceState

        Public Sub New(deviceId As DebugDeviceId)
            Root = New Panel() With {.Height = 70, .Margin = New Padding(0, 0, 0, 8), .BackColor = UiTheme.CardBackground, .BorderStyle = BorderStyle.FixedSingle, .Padding = New Padding(10, 8, 8, 8)}
            statusDot = New Label() With {.Text = "●", .AutoSize = True, .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold), .Location = New Point(10, 20)}
            Dim nameText As New Label() With {.Text = DebugProbeService.GetDeviceName(deviceId), .AutoSize = True, .Font = UiTheme.UiFontBold, .Location = New Point(30, 10), .ForeColor = UiTheme.TextPrimary}
            detailText = New Label() With {.AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Location = New Point(30, 34)}
            statusText = New Label() With {.AutoSize = True, .Font = UiTheme.UiFontBold, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(0, 10)}
            timeText = New Label() With {.AutoSize = True, .ForeColor = UiTheme.TextSecondary, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(0, 36)}
            TestButton = New Button() With {.Text = "测试连接", .Size = New Size(90, 30), .Anchor = AnchorStyles.Top Or AnchorStyles.Right}
            UiTheme.ApplyButtonStyle(TestButton, UiTheme.ButtonRole.Neutral)
            Root.Controls.Add(statusDot)
            Root.Controls.Add(nameText)
            Root.Controls.Add(detailText)
            Root.Controls.Add(statusText)
            Root.Controls.Add(timeText)
            Root.Controls.Add(TestButton)
            AddHandler Root.Resize, Sub(sender, args) PositionRightControls()
            PositionRightControls()
        End Sub

        Public ReadOnly Property CurrentState As DebugDeviceState
            Get
                Return deviceState
            End Get
        End Property

        Public Sub SetTesting(testing As Boolean)
            If testing Then
                deviceState = DebugDeviceState.Testing
                statusDot.ForeColor = UiTheme.Primary
                statusText.Text = "检查中"
                statusText.ForeColor = UiTheme.Primary
                TestButton.Enabled = False
            Else
                TestButton.Enabled = True
            End If
        End Sub

        Public Sub SetResult(result As DebugDeviceResult)
            deviceState = result.State
            statusText.Text = GetStateText(result.State)
            statusText.ForeColor = GetStateColor(result.State)
            statusDot.ForeColor = GetStateColor(result.State)
            detailText.Text = result.InterfaceDescription & " · " & result.Message
            timeText.Text = If(result.CheckedAt = DateTime.MinValue, "尚未检查", result.CheckedAt.ToString("HH:mm:ss") & " · " & result.ElapsedMilliseconds.ToString() & " ms")
            TestButton.Enabled = result.State <> DebugDeviceState.Testing
        End Sub

        Private Sub PositionRightControls()
            TestButton.Location = New Point(Math.Max(250, Root.ClientSize.Width - TestButton.Width - 8), 18)
            statusText.Location = New Point(Math.Max(170, Root.ClientSize.Width - 220), 9)
            timeText.Location = New Point(Math.Max(170, Root.ClientSize.Width - 220), 36)
        End Sub

        Private Shared Function GetStateText(state As DebugDeviceState) As String
            Select Case state
                Case DebugDeviceState.Connected : Return "已连接"
                Case DebugDeviceState.Managed : Return "已管理"
                Case DebugDeviceState.Failed : Return "失败"
                Case DebugDeviceState.Testing : Return "检查中"
                Case DebugDeviceState.Unavailable : Return "不可用"
                Case Else : Return "待检查"
            End Select
        End Function

        Private Shared Function GetStateColor(state As DebugDeviceState) As Color
            Select Case state
                Case DebugDeviceState.Connected, DebugDeviceState.Managed : Return UiTheme.Success
                Case DebugDeviceState.Failed : Return UiTheme.Danger
                Case DebugDeviceState.Testing : Return UiTheme.Primary
                Case DebugDeviceState.Unavailable : Return UiTheme.Warning
                Case Else : Return UiTheme.TextSecondary
            End Select
        End Function
    End Class
End Class
