Imports System.Drawing
Imports System.Globalization
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class FrmNgRetest
    Inherits Form

    Private Enum NgRetestLookupState
        NotQueried
        Found
        NotFound
        Failed
    End Enum

    Private ReadOnly meterService As New Retest3562Service()
    Private ReadOnly dataService As New NgRetestDataService()
    Private txtBarcode As TextBox
    Private btnLookup As Button
    Private btnRetest As Button
    Private btnRetrySave As Button
    Private chkOverwrite As CheckBox
    Private chkDebugMode As CheckBox
    Private lblMeterStatus As Label
    Private lblLookupStatus As Label
    Private lblHistoryResult As Label
    Private lblHistoryVoltage As Label
    Private lblHistoryResistance As Label
    Private lblHistoryTime As Label
    Private lblHistoryTray As Label
    Private lblHistoryPosition As Label
    Private lblRetestResult As Label
    Private lblRetestVoltage As Label
    Private lblRetestResistance As Label
    Private lblRetestTime As Label
    Private lblRetestStatus As Label
    Private currentSource As NgRetestSource
    Private currentMeasurement As RetestMeasurement
    Private pendingSaveRequest As NgRetestSaveRequest
    Private lookupState As NgRetestLookupState = NgRetestLookupState.NotQueried
    Private isBusy As Boolean

    Public Sub New()
        Text = "NG复测"
        Name = "FrmNgRetest"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(900, 560)
        Size = New Size(1040, 650)
        FormBorderStyle = FormBorderStyle.Sizable
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        lblHistoryResult = CreateValueLabel("等待查询")
        lblHistoryVoltage = CreateValueLabel("—")
        lblHistoryResistance = CreateValueLabel("—")
        lblHistoryTime = CreateValueLabel("—")
        lblHistoryTray = CreateValueLabel("—")
        lblHistoryPosition = CreateValueLabel("—")
        lblRetestResult = CreateValueLabel("—")
        lblRetestVoltage = CreateValueLabel("—")
        lblRetestResistance = CreateValueLabel("—")
        lblRetestTime = CreateValueLabel("—")
        lblRetestStatus = CreateValueLabel("等待复测")

        txtBarcode = New TextBox() With {.Name = "txtRetestBarcode", .Width = 470, .Font = New Font(Font.FontFamily, 11.0F)}
        btnLookup = New Button() With {.Text = "查询", .Width = 92, .Height = 30, .Margin = New Padding(10, 0, 0, 0)}
        lblMeterStatus = New Label() With {.Text = "复测3562：未连接", .AutoSize = True, .ForeColor = Color.DarkOrange, .Margin = New Padding(0, 4, 24, 0)}
        lblLookupStatus = New Label() With {.Text = "请扫描电池条码。", .AutoSize = True, .Margin = New Padding(0, 4, 0, 0)}

        Dim queryPanel As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2}
        queryPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
        queryPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Dim queryRow As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(2, 4, 2, 0)
        }
        queryRow.Controls.Add(New Label() With {.Text = "电池条码", .AutoSize = True, .Margin = New Padding(0, 7, 10, 0)})
        queryRow.Controls.Add(txtBarcode)
        queryRow.Controls.Add(btnLookup)

        Dim statusRow As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(2, 4, 2, 0)
        }
        statusRow.Controls.Add(lblMeterStatus)
        statusRow.Controls.Add(lblLookupStatus)
        queryPanel.Controls.Add(queryRow, 0, 0)
        queryPanel.Controls.Add(statusRow, 0, 1)

        Dim cards As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1, .Margin = New Padding(0)}
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Dim historyCard As GroupBox = CreateMetricCard(
            "历史 NG",
            New String() {"历史判定", "历史电压", "历史内阻", "原测试时间", "托盘号", "托盘位置 / 通道"},
            New Label() {lblHistoryResult, lblHistoryVoltage, lblHistoryResistance, lblHistoryTime, lblHistoryTray, lblHistoryPosition})
        Dim retestCard As GroupBox = CreateMetricCard(
            "本次复测",
            New String() {"复测判定", "复测电压", "复测内阻", "采集时间", "采集状态"},
            New Label() {lblRetestResult, lblRetestVoltage, lblRetestResistance, lblRetestTime, lblRetestStatus})
        historyCard.Margin = New Padding(4, 2, 8, 4)
        retestCard.Margin = New Padding(8, 2, 4, 4)
        cards.Controls.Add(historyCard, 0, 0)
        cards.Controls.Add(retestCard, 1, 0)

        chkDebugMode = New CheckBox() With {
            .Text = "调试模式（无历史时仅采集显示）",
            .AutoSize = True,
            .Checked = False,
            .Enabled = False,
            .Margin = New Padding(4, 8, 20, 0)
        }
        chkOverwrite = New CheckBox() With {
            .Text = "覆盖历史",
            .AutoSize = True,
            .Checked = False,
            .Enabled = False,
            .Margin = New Padding(0, 8, 20, 0)
        }
        btnRetest = New Button() With {.Text = "复测", .Size = New Size(120, 34), .Enabled = False, .Margin = New Padding(0, 2, 10, 0)}
        btnRetrySave = New Button() With {.Text = "重试保存", .Size = New Size(120, 34), .Enabled = False, .Margin = New Padding(0, 2, 0, 0)}
        Dim actions As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(2, 2, 2, 0),
            .AutoScroll = True
        }
        actions.Controls.Add(chkDebugMode)
        actions.Controls.Add(chkOverwrite)
        actions.Controls.Add(btnRetest)
        actions.Controls.Add(btnRetrySave)

        Dim layout As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3, .Padding = New Padding(14)}
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 82.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 52.0F))
        layout.Controls.Add(queryPanel, 0, 0)
        layout.Controls.Add(cards, 0, 1)
        layout.Controls.Add(actions, 0, 2)
        Controls.Add(layout)

        AddHandler txtBarcode.KeyDown, AddressOf BarcodeKeyDown
        AddHandler btnLookup.Click, AddressOf LookupClick
        AddHandler btnRetest.Click, AddressOf RetestClick
        AddHandler btnRetrySave.Click, AddressOf RetrySaveClick
        AddHandler chkDebugMode.CheckedChanged, AddressOf DebugModeChanged
        AddHandler FormClosing, AddressOf FormClosingHandler
        UpdateActionState()
    End Sub

    Private Shared Function CreateValueLabel(ByVal initialText As String) As Label
        Return New Label() With {
            .Text = initialText,
            .Dock = DockStyle.Fill,
            .AutoSize = False,
            .AutoEllipsis = True,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(4, 0, 4, 0),
            .Font = New Font(SystemFonts.MessageBoxFont.FontFamily, 10.0F, FontStyle.Bold)
        }
    End Function

    Private Function CreateMetricCard(ByVal title As String, ByVal captions As String(), ByVal values As Label()) As GroupBox
        Dim card As New GroupBox() With {.Text = title, .Dock = DockStyle.Fill, .Padding = New Padding(8)}
        Dim grid As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = captions.Length, .Padding = New Padding(4)}
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36.0F))
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 64.0F))
        For index As Integer = 0 To captions.Length - 1
            grid.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / captions.Length))
            grid.Controls.Add(New Label() With {
                .Text = captions(index),
                .Dock = DockStyle.Fill,
                .AutoSize = False,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(4, 0, 2, 0)
            }, 0, index)
            grid.Controls.Add(values(index), 1, index)
        Next
        card.Controls.Add(grid)
        Return card
    End Function

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        txtBarcode.Focus()
        Try
            Dim portName As String = "COM" & (Initsetting.Retest3562Port + 1).ToString()
            meterService.Connect(portName)
            lblMeterStatus.Text = "复测3562：已连接（" & portName & "）"
            lblMeterStatus.ForeColor = Color.DarkGreen
        Catch ex As Exception
            lblMeterStatus.Text = "复测3562：连接失败 - " & ex.Message
            lblMeterStatus.ForeColor = Color.Red
        End Try
        UpdateActionState()
    End Sub

    Private Sub BarcodeKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            LookupClick(sender, EventArgs.Empty)
        End If
    End Sub

    Private Async Sub LookupClick(ByVal sender As Object, ByVal e As EventArgs)
        If isBusy Then Return
        Dim code As String = txtBarcode.Text.Trim()
        If code = String.Empty Then
            MessageBox.Show("请先扫描电池条码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ResetForLookup()
        lookupState = NgRetestLookupState.NotQueried
        lblLookupStatus.Text = "正在查询：" & code
        SetBusy(True)
        Try
            currentSource = Await Task.Run(Function() dataService.LoadByBatteryCode(code))
            If currentSource Is Nothing Then
                lookupState = NgRetestLookupState.NotFound
                SetHistoryValues("未找到正式 NG 历史", "—", "—", "—", "—", "—")
                lblLookupStatus.Text = "查无正式 NG 历史；开启调试模式可仅采集查看。"
                Return
            End If

            lookupState = NgRetestLookupState.Found
            SetHistoryValues(
                currentSource.OriginalResult,
                String.Format(CultureInfo.CurrentCulture, "{0:F2} mV", currentSource.OriginalVoltage * 1000.0F),
                String.Format(CultureInfo.CurrentCulture, "{0:F2} mΩ", currentSource.OriginalResistance),
                FormatDate(currentSource.OriginalTestTime),
                currentSource.TrayCode,
                String.Format(CultureInfo.CurrentCulture, "{0} / 通道 {1}", currentSource.TrayPosition, currentSource.ChannelNo))
            lblHistoryResult.ForeColor = Color.Red
            lblLookupStatus.Text = "已找到正式 NG 历史。"
            WriteOperationLog("成功", "查询NG复测记录", currentSource.BatteryCode, "已查询原始测试结果。",
                              String.Format(CultureInfo.InvariantCulture, "TestResultId={0}；判定={1}；托盘={2}；位置={3}；原始电压={4:F2} mV；原始内阻={5:F2} mΩ。",
                                            currentSource.SourceTestResultId, currentSource.OriginalResult, currentSource.TrayCode,
                                            currentSource.TrayPosition, currentSource.OriginalVoltage * 1000.0F, currentSource.OriginalResistance))
        Catch ex As Exception
            currentSource = Nothing
            lookupState = NgRetestLookupState.Failed
            SetHistoryValues("查询失败", "—", "—", "—", "—", "—")
            lblLookupStatus.Text = "查询失败：" & ex.Message
            WriteOperationLog("失败", "查询NG复测记录", code, "NG复测记录查询失败。", ex.Message)
            MessageBox.Show(ex.Message, "NG复测查询失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            SetBusy(False)
            txtBarcode.Focus()
            txtBarcode.SelectAll()
        End Try
    End Sub

    Private Async Sub RetestClick(ByVal sender As Object, ByVal e As EventArgs)
        If isBusy OrElse Not CanMeasure() Then Return
        If Not meterService.IsConnected Then
            MessageBox.Show("复测3562未连接，不能测量。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        SetBusy(True)
        currentMeasurement = Nothing
        pendingSaveRequest = Nothing
        SetRetestValues("—", "—", "—", "—", "正在采集……")
        Try
            currentMeasurement = Await Task.Run(Function() meterService.Measure())
            DisplayMeasurement(currentMeasurement)

            If lookupState = NgRetestLookupState.NotFound AndAlso chkDebugMode.Checked Then
                lblRetestStatus.Text = "调试采集完成 · 仅显示未入库"
                Return
            End If

            If lookupState <> NgRetestLookupState.Found OrElse currentSource Is Nothing Then Return
            pendingSaveRequest = New NgRetestSaveRequest() With {
                .Source = currentSource,
                .Measurement = currentMeasurement,
                .RetestOperationId = Guid.NewGuid(),
                .OverwriteOriginal = chkOverwrite.Checked,
                .OperatorName = If(Initsetting.curOperator, operatorName),
                .MeterPort = meterService.PortName
            }
            Await SavePendingRetestAsync()
        Catch ex As Exception
            lblRetestStatus.Text = "采集失败：" & ex.Message
            If lookupState = NgRetestLookupState.Found AndAlso currentSource IsNot Nothing Then
                WriteOperationLog("失败", "执行NG复测", currentSource.BatteryCode, "NG复测采集失败。", ex.Message)
            End If
            MessageBox.Show(ex.Message, "NG复测采集失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub RetrySaveClick(ByVal sender As Object, ByVal e As EventArgs)
        If isBusy OrElse pendingSaveRequest Is Nothing Then Return
        SetBusy(True)
        Try
            Await SavePendingRetestAsync()
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Function SavePendingRetestAsync() As Task
        If pendingSaveRequest Is Nothing Then Return
        Dim request As NgRetestSaveRequest = pendingSaveRequest
        lblRetestStatus.Text = "正在保存复测记录……"
        Try
            Await Task.Run(Sub() dataService.SaveRetest(request))
            pendingSaveRequest = Nothing
            lblRetestStatus.Text = If(request.OverwriteOriginal, "已记录复测并覆盖历史", "已记录复测，历史未覆盖")
            WriteOperationLog("成功", "完成NG复测", request.Source.BatteryCode, "NG复测数据已保存。",
                              String.Format(CultureInfo.InvariantCulture, "TestResultId={0}；复测判定={1}；电压={2:F2} mV；内阻={3:F2} mΩ；覆盖原数据={4}；仪表端口={5}。",
                                            request.Source.SourceTestResultId, request.Measurement.Result,
                                            request.Measurement.Voltage * 1000.0F, request.Measurement.Resistance,
                                            request.OverwriteOriginal, request.MeterPort))
        Catch ex As Exception
            lblRetestStatus.Text = "保存失败，测量值已保留；请重试保存。"
            WriteOperationLog("失败", "保存NG复测", request.Source.BatteryCode, "NG复测结果未确认保存。", ex.Message)
            MessageBox.Show(ex.Message & Environment.NewLine & "复测值已保留；点击“重试保存”不会重新采集。", "NG复测保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function

    Private Sub DebugModeChanged(ByVal sender As Object, ByVal e As EventArgs)
        If chkDebugMode.Checked AndAlso lookupState = NgRetestLookupState.NotFound AndAlso currentMeasurement Is Nothing Then
            lblRetestStatus.Text = "调试模式已开启：结果仅显示，不写入数据库。"
        ElseIf currentMeasurement Is Nothing Then
            lblRetestStatus.Text = "等待复测"
        End If
        UpdateActionState()
    End Sub

    Private Sub ResetForLookup()
        currentSource = Nothing
        currentMeasurement = Nothing
        pendingSaveRequest = Nothing
        lookupState = NgRetestLookupState.NotQueried
        chkOverwrite.Checked = False
        chkDebugMode.Checked = False
        SetHistoryValues("等待查询", "—", "—", "—", "—", "—")
        SetRetestValues("—", "—", "—", "—", "等待复测")
        UpdateActionState()
    End Sub

    Private Sub SetHistoryValues(ByVal result As String, ByVal voltage As String, ByVal resistance As String,
                                 ByVal testTime As String, ByVal trayCode As String, ByVal trayPosition As String)
        lblHistoryResult.Text = result
        lblHistoryVoltage.Text = voltage
        lblHistoryResistance.Text = resistance
        lblHistoryTime.Text = testTime
        lblHistoryTray.Text = trayCode
        lblHistoryPosition.Text = trayPosition
        lblHistoryResult.ForeColor = If(String.Equals(result, "NG", StringComparison.OrdinalIgnoreCase), Color.Red, SystemColors.ControlText)
    End Sub

    Private Sub SetRetestValues(ByVal result As String, ByVal voltage As String, ByVal resistance As String,
                                ByVal measuredAt As String, ByVal status As String)
        lblRetestResult.Text = result
        lblRetestVoltage.Text = voltage
        lblRetestResistance.Text = resistance
        lblRetestTime.Text = measuredAt
        lblRetestStatus.Text = status
        lblRetestResult.ForeColor = SystemColors.ControlText
    End Sub

    Private Sub DisplayMeasurement(ByVal measurement As RetestMeasurement)
        lblRetestResult.Text = measurement.Result
        lblRetestResult.ForeColor = If(String.Equals(measurement.Result, "OK", StringComparison.OrdinalIgnoreCase), Color.DarkGreen, Color.Red)
        lblRetestVoltage.Text = If(measurement.VoltageMeasured,
                                   String.Format(CultureInfo.CurrentCulture, "{0:F2} mV", measurement.Voltage * 1000.0F),
                                   "未采集")
        lblRetestResistance.Text = String.Format(CultureInfo.CurrentCulture, "{0:F2} mΩ", measurement.Resistance)
        lblRetestTime.Text = FormatDate(measurement.MeasuredAt)
        lblRetestStatus.Text = "已采集，正在保存……"
    End Sub

    Private Shared Function FormatDate(ByVal value As DateTime) As String
        Return If(value = DateTime.MinValue, "—", value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture))
    End Function

    Private Function CanMeasure() As Boolean
        Return meterService.IsConnected AndAlso pendingSaveRequest Is Nothing AndAlso
            (lookupState = NgRetestLookupState.Found OrElse (lookupState = NgRetestLookupState.NotFound AndAlso chkDebugMode.Checked))
    End Function

    Private Sub SetBusy(ByVal busy As Boolean)
        isBusy = busy
        txtBarcode.Enabled = Not busy AndAlso pendingSaveRequest Is Nothing
        btnLookup.Enabled = Not busy AndAlso pendingSaveRequest Is Nothing
        UpdateActionState()
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    Private Sub UpdateActionState()
        If chkOverwrite Is Nothing OrElse chkDebugMode Is Nothing OrElse btnRetest Is Nothing OrElse btnRetrySave Is Nothing Then Return
        chkOverwrite.Enabled = Not isBusy AndAlso pendingSaveRequest Is Nothing AndAlso lookupState = NgRetestLookupState.Found
        chkDebugMode.Enabled = Not isBusy AndAlso pendingSaveRequest Is Nothing AndAlso lookupState = NgRetestLookupState.NotFound
        btnRetest.Enabled = Not isBusy AndAlso CanMeasure()
        btnRetrySave.Enabled = Not isBusy AndAlso pendingSaveRequest IsNot Nothing
    End Sub

    Private Sub WriteOperationLog(ByVal result As String, ByVal actionName As String, ByVal businessKey As String,
                                  ByVal summary As String, ByVal detail As String)
        Try
            BusinessLogService.Write("操作", result, actionName, businessKey, summary, detail, "NG复测")
        Catch
            'Logging must not change measurement, lookup, or persistence outcomes.
        End Try
    End Sub

    Private Sub FormClosingHandler(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        meterService.Dispose()
    End Sub
End Class
