Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions

Public Class FrmManualTest
    Inherits Form

    Private ReadOnly channelButtons As New Dictionary(Of Integer, CheckBox)()
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()
    Private ReadOnly channelToolTip As New ToolTip()

    Private commandPanel As FlowLayoutPanel
    Private matrixPanel As TableLayoutPanel
    Private resultGrid As DataGridView
    Private lblSummary As Label
    Private lblStatus As Label
    Private txtRangeStart As TextBox
    Private txtRangeEnd As TextBox
    Private txtChannelList As TextBox
    Private btnStart As Button
    Private btnClose As Button
    Private btnClampClose As Button
    Private btnClampOpen As Button
    Private isManualTesting As Boolean

    Public Sub New()
        InitializeManualForm()
    End Sub

    Private Sub InitializeManualForm()
        Me.Text = "手动测试"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(1475, 975)
        Me.MinimumSize = New Size(1225, 850)
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.ColumnCount = 1
        root.RowCount = 4
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 112))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 58))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 42))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        Me.Controls.Add(root)

        root.Controls.Add(BuildCommandPanel(), 0, 0)
        root.Controls.Add(BuildMatrixPanel(), 0, 1)
        root.Controls.Add(BuildResultGrid(), 0, 2)
        root.Controls.Add(BuildBottomPanel(), 0, 3)

        UpdateSelectionSummary()
    End Sub

    Private Function BuildCommandPanel() As Control
        commandPanel = New FlowLayoutPanel()
        commandPanel.Dock = DockStyle.Fill
        commandPanel.Padding = New Padding(10)
        commandPanel.AutoScroll = True

        commandPanel.Controls.Add(CreateButton("全选", AddressOf SelectAllChannels))
        commandPanel.Controls.Add(CreateButton("清空", AddressOf ClearSelection))
        commandPanel.Controls.Add(CreateButton("反选", AddressOf InvertSelection))
        commandPanel.Controls.Add(CreateButton("G1全选", AddressOf SelectG1Channels))
        commandPanel.Controls.Add(CreateButton("G2全选", AddressOf SelectG2Channels))

        btnClampClose = CreateButton("夹具压合", AddressOf ClampClose)
        btnClampOpen = CreateButton("夹具松开", AddressOf ClampOpen)
        commandPanel.Controls.Add(btnClampClose)
        commandPanel.Controls.Add(btnClampOpen)

        commandPanel.Controls.Add(New Label() With {.Text = "起始", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
        txtRangeStart = New TextBox() With {.Width = 56}
        commandPanel.Controls.Add(txtRangeStart)
        commandPanel.Controls.Add(New Label() With {.Text = "结束", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
        txtRangeEnd = New TextBox() With {.Width = 56}
        commandPanel.Controls.Add(txtRangeEnd)
        commandPanel.Controls.Add(CreateButton("添加范围", AddressOf AddRangeSelection))

        commandPanel.Controls.Add(New Label() With {.Text = "零星通道", .AutoSize = True, .Padding = New Padding(10, 8, 0, 0)})
        txtChannelList = New TextBox() With {.Width = 260}
        commandPanel.Controls.Add(txtChannelList)
        commandPanel.Controls.Add(CreateButton("添加列表", AddressOf AddListSelection))

        lblSummary = New Label() With {.AutoSize = True, .Padding = New Padding(10, 8, 0, 0)}
        commandPanel.Controls.Add(lblSummary)

        Return commandPanel
    End Function

    Private Sub ClampClose(sender As Object, e As EventArgs)
        OperateClamp(True)
    End Sub

    Private Sub ClampOpen(sender As Object, e As EventArgs)
        OperateClamp(False)
    End Sub

    Private Sub OperateClamp(closeClamp As Boolean)
        If isManualTesting OrElse isRuning Then
            MsgBox("测试进行中，暂不能操作夹具。", vbOKOnly, "提示")
            Return
        End If

        If plcAutomaticModeActive Then
            lblStatus.Text = "PLC由自动测试流程占用"
            MsgBox("自动测试流程运行中，暂不能手动操作夹具。", vbOKOnly, "夹具控制")
            Return
        End If

        If Not plcConnected OrElse plcController Is Nothing Then
            lblStatus.Text = "PLC未连接"
            MsgBox("PLC未连接，无法控制夹具。", vbOKOnly, "夹具控制")
            Return
        End If

        Try
            Dim commandAccepted As Boolean = If(closeClamp, plcController.CloseTray(), plcController.OpenTray())
            Dim actualClosed As Boolean = plcController.ReadTrayStatus()
            Dim expectedClosed As Boolean = closeClamp
            If commandAccepted AndAlso actualClosed = expectedClosed Then
                lblStatus.Text = If(closeClamp, "夹具已压合", "夹具已松开")
                Return
            End If

            lblStatus.Text = "夹具动作失败"
            MsgBox(If(closeClamp, "夹具压合失败，请检查 PLC 和气缸。", "夹具松开失败，请检查 PLC 和气缸。"), vbOKOnly, "夹具控制")
        Catch ex As Exception
            lblStatus.Text = "夹具通讯失败"
            WriteManualLog("夹具控制失败：" & ex.Message)
            MsgBox("夹具控制失败：" & ex.Message, vbOKOnly, "夹具控制")
        End Try
    End Sub

    Private Function CreateButton(text As String, handler As EventHandler) As Button
        Dim button As New Button()
        button.Text = text
        button.Width = 82
        button.Height = 30
        AddHandler button.Click, handler
        Return button
    End Function

    Private Function BuildMatrixPanel() As Control
        matrixPanel = New TableLayoutPanel()
        matrixPanel.Dock = DockStyle.Fill
        matrixPanel.ColumnCount = 16
        matrixPanel.RowCount = 16
        matrixPanel.Padding = New Padding(10)

        For i As Integer = 0 To 15
            matrixPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 6.25F))
            matrixPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 6.25F))
        Next

        For channel As Integer = 1 To 256
            Dim box As New CheckBox()
            box.Appearance = Appearance.Button
            box.Text = channel.ToString()
            box.TextAlign = ContentAlignment.MiddleCenter
            box.Dock = DockStyle.Fill
            box.Margin = New Padding(2)
            box.Tag = channel
            box.BackColor = GetUnselectedChannelColor(channel)
            box.FlatStyle = FlatStyle.Standard

            Dim localChannel As Integer = If(channel <= 128, channel, channel - 128)
            If channel <= 128 Then
                channelToolTip.SetToolTip(box, "G1 CH" & localChannel.ToString())
            Else
                channelToolTip.SetToolTip(box, "G2 本地 CH" & localChannel.ToString())
            End If

            AddHandler box.CheckedChanged, AddressOf ChannelCheckedChanged
            channelButtons(channel) = box
            matrixPanel.Controls.Add(box, (channel - 1) Mod 16, (channel - 1) \ 16)
        Next

        Return matrixPanel
    End Function

    Private Function BuildResultGrid() As Control
        resultGrid = New DataGridView()
        resultGrid.Dock = DockStyle.Fill
        resultGrid.AllowUserToAddRows = False
        resultGrid.AllowUserToDeleteRows = False
        resultGrid.ReadOnly = True
        resultGrid.RowHeadersVisible = False
        resultGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        resultGrid.Columns.Add("Channel", "通道")
        resultGrid.Columns.Add("Group", "分组")
        resultGrid.Columns.Add("Voltage", "电压 mV")
        resultGrid.Columns.Add("Resistance", "内阻 mΩ")
        resultGrid.Columns.Add("Result", "判定")
        Return resultGrid
    End Function

    Private Function BuildBottomPanel() As Control
        Dim panel As New TableLayoutPanel()
        panel.Dock = DockStyle.Fill
        panel.ColumnCount = 3
        panel.RowCount = 1
        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 112))
        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 92))
        panel.Padding = New Padding(10, 7, 10, 7)

        lblStatus = New Label()
        lblStatus.Text = "空闲"
        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        panel.Controls.Add(lblStatus, 0, 0)

        btnStart = New Button()
        btnStart.Text = "开始测试"
        btnStart.Dock = DockStyle.Fill
        AddHandler btnStart.Click, AddressOf StartManualTest
        panel.Controls.Add(btnStart, 1, 0)

        btnClose = New Button()
        btnClose.Text = "关闭"
        btnClose.Dock = DockStyle.Fill
        AddHandler btnClose.Click, Sub(sender, e) Me.Close()
        panel.Controls.Add(btnClose, 2, 0)

        Return panel
    End Function

    Private Sub ChannelCheckedChanged(sender As Object, e As EventArgs)
        Dim box As CheckBox = CType(sender, CheckBox)
        Dim channel As Integer = CInt(box.Tag)

        If box.Checked Then
            selectedChannels.Add(channel)
            box.BackColor = Color.LightSkyBlue
        Else
            selectedChannels.Remove(channel)
            box.BackColor = GetUnselectedChannelColor(channel)
        End If

        UpdateSelectionSummary()
    End Sub

    Private Function GetUnselectedChannelColor(channel As Integer) As Color
        If channel <= 128 Then
            Return Color.AliceBlue
        End If
        Return Color.Honeydew
    End Function

    Private Sub SetChannelChecked(channel As Integer, checked As Boolean)
        If channelButtons.ContainsKey(channel) Then
            channelButtons(channel).Checked = checked
        End If
    End Sub

    Private Sub SelectAllChannels(sender As Object, e As EventArgs)
        For channel As Integer = 1 To 256
            SetChannelChecked(channel, True)
        Next
    End Sub

    Private Sub ClearSelection(sender As Object, e As EventArgs)
        For channel As Integer = 1 To 256
            SetChannelChecked(channel, False)
        Next
    End Sub

    Private Sub InvertSelection(sender As Object, e As EventArgs)
        For channel As Integer = 1 To 256
            SetChannelChecked(channel, Not channelButtons(channel).Checked)
        Next
    End Sub

    Private Sub SelectG1Channels(sender As Object, e As EventArgs)
        For channel As Integer = 1 To 128
            SetChannelChecked(channel, True)
        Next
    End Sub

    Private Sub SelectG2Channels(sender As Object, e As EventArgs)
        For channel As Integer = 129 To 256
            SetChannelChecked(channel, True)
        Next
    End Sub

    Private Sub UpdateSelectionSummary()
        Dim total As Integer = selectedChannels.Count
        Dim g1 As Integer = selectedChannels.Where(Function(channel) channel <= 128).Count()
        Dim g2 As Integer = selectedChannels.Where(Function(channel) channel > 128).Count()
        Dim expectedHioki As Integer = g1 + g2
        Dim expectedDmm As Integer = g2 + g1

        If lblSummary IsNot Nothing Then
            lblSummary.Text = String.Format("已选 {0}/256；G1 {1}；G2 {2}；3562 {3}；34461 {4}", total, g1, g2, expectedHioki, expectedDmm)
        End If
    End Sub

    Private Sub AddRangeSelection(sender As Object, e As EventArgs)
        Dim startChannel As Integer
        Dim endChannel As Integer

        If Not Integer.TryParse(txtRangeStart.Text.Trim(), startChannel) OrElse
           Not Integer.TryParse(txtRangeEnd.Text.Trim(), endChannel) Then
            MsgBox("起始通道和结束通道必须是整数。", vbOKOnly, "提示")
            Return
        End If

        If startChannel < 1 OrElse startChannel > 256 OrElse endChannel < 1 OrElse endChannel > 256 Then
            MsgBox("通道范围必须在 1 到 256 之间。", vbOKOnly, "提示")
            Return
        End If

        If startChannel > endChannel Then
            MsgBox("起始通道不能大于结束通道。", vbOKOnly, "提示")
            Return
        End If

        For channel As Integer = startChannel To endChannel
            SetChannelChecked(channel, True)
        Next
    End Sub

    Private Function ParseChannelList(input As String, ByRef invalidItems As List(Of String)) As List(Of Integer)
        invalidItems = New List(Of String)()
        Dim channels As New List(Of Integer)()
        Dim normalized As String = input.ToUpperInvariant().
            Replace("CH", "").
            Replace("，", ",").
            Replace("、", ",").
            Replace(";", ",").
            Replace("；", ",")

        Dim parts As String() = Regex.Split(normalized, "[,\s]+")
        For Each raw As String In parts
            Dim token As String = raw.Trim()
            If token = "" Then
                Continue For
            End If

            Dim channel As Integer
            If Not Integer.TryParse(token, channel) OrElse channel < 1 OrElse channel > 256 Then
                invalidItems.Add(raw)
            Else
                channels.Add(channel)
            End If
        Next

        Return channels.Distinct().OrderBy(Function(channel) channel).ToList()
    End Function

    Private Sub AddListSelection(sender As Object, e As EventArgs)
        Dim invalidItems As List(Of String) = Nothing
        Dim channels As List(Of Integer) = ParseChannelList(txtChannelList.Text, invalidItems)

        If invalidItems.Count > 0 Then
            MsgBox("以下通道无效：" & String.Join(", ", invalidItems), vbOKOnly, "提示")
            Return
        End If

        If channels.Count = 0 Then
            MsgBox("请输入至少一个通道。", vbOKOnly, "提示")
            Return
        End If

        For Each channel As Integer In channels
            SetChannelChecked(channel, True)
        Next
    End Sub

    Private Function GetSelectedChannels() As List(Of Integer)
        Return selectedChannels.OrderBy(Function(channel) channel).ToList()
    End Function

    Private Sub StartManualTest(sender As Object, e As EventArgs)
        If Not AuthorizationService.RequireAuthenticatedUser(Me) Then Return
        If plcAutomaticModeActive Then
            MsgBox("PLC 自动测试流程正在运行，不能启动调试测试。", vbOKOnly, "提示")
            Return
        End If
        Dim channels As List(Of Integer) = GetSelectedChannels()
        If channels.Count = 0 Then
            MsgBox("请选择至少一个通道。", vbOKOnly, "提示")
            Return
        End If

        If isRuning OrElse isManualTesting Then
            MsgBox("系统正在测试中，请稍后。", vbOKOnly, "提示")
            Return
        End If

        If CheckCom_SCM() <> 1 Then
            MsgBox("中位机串口通讯失败，无法启动手动测试。", vbOKOnly, "错误")
            Return
        End If

        Try
            SetTestingState(True, "测试中...")
            WriteManualLog("开始手动测试：" & String.Join(",", channels))

            Dim formalRunner As New FormalTestRunner(AddressOf WriteManualLog)
            formalRunner.RunSparseMeasurement(channels)

            DisplayManualTestResult(channels)
            WriteManualLog("手动测试完成。")
            lblStatus.Text = "测试完成"
        Catch ex As Exception
            lblStatus.Text = "测试失败"
            WriteManualLog("手动测试失败：" & ex.ToString())
            MsgBox("手动测试失败：" & ex.Message, vbOKOnly, "错误")
        Finally
            SetTestingState(False, lblStatus.Text)
        End Try
    End Sub

    Private Sub SetTestingState(testing As Boolean, statusText As String)
        isManualTesting = testing
        isRuning = testing
        lblStatus.Text = statusText

        btnStart.Enabled = Not testing
        btnClose.Enabled = Not testing
        If commandPanel IsNot Nothing Then
            For Each control As Control In commandPanel.Controls
                control.Enabled = Not testing
            Next
        End If
        If matrixPanel IsNot Nothing Then
            For Each control As Control In matrixPanel.Controls
                control.Enabled = Not testing
            Next
        End If
    End Sub

    Private Sub DisplayManualTestResult(channels As IEnumerable(Of Integer))
        resultGrid.Rows.Clear()

        For Each channel As Integer In channels.OrderBy(Function(ch) ch)
            Dim index0 As Integer = channel - 1
            Dim voltageMv As Single = RecieveData(index0).Voltage * 1000
            Dim resistance As Single = RecieveData(index0).Resistance
            Dim resultText As String = If(IsNG(RecieveData(index0).Voltage, resistance) = 0, "OK", "NG")
            Dim groupText As String = If(channel <= 128, "G1", "G2 本地CH" & (channel - 128).ToString())

            resultGrid.Rows.Add("CH" & channel.ToString(),
                                groupText,
                                voltageMv.ToString("0.0000"),
                                resistance.ToString("0.0000"),
                                resultText)
        Next
    End Sub

    Private Sub WriteManualLog(message As String)
        AppLog.Write("ManualTest", message)
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If isManualTesting Then
            MsgBox("手动测试正在进行，请等待测试完成。", vbOKOnly, "提示")
            e.Cancel = True
            Return
        End If

        MyBase.OnFormClosing(e)
    End Sub
End Class
