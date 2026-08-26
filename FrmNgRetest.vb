Imports System.Drawing
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class FrmNgRetest
    Inherits Form

    Private ReadOnly meterService As New Retest3562Service()
    Private ReadOnly dataService As New NgRetestDataService()
    Private txtBarcode As TextBox
    Private btnLookup As Button
    Private btnRetest As Button
    Private chkOverwrite As CheckBox
    Private lblMeterStatus As Label
    Private lblSource As Label
    Private lblResult As Label
    Private currentSource As NgRetestSource
    Private isBusy As Boolean

    Public Sub New()
        Text = "NG复测"
        Name = "FrmNgRetest"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(760, 480)
        Size = New Size(860, 560)
        FormBorderStyle = FormBorderStyle.Sizable
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim title As New Label() With {.Text = "NG电池单通道复测", .Font = New Font(Font, FontStyle.Bold), .Location = New Point(24, 20), .AutoSize = True}
        Dim barcodeLabel As New Label() With {.Text = "电池条码", .Location = New Point(24, 70), .AutoSize = True}
        txtBarcode = New TextBox() With {.Name = "txtRetestBarcode", .Location = New Point(100, 66), .Width = 430}
        btnLookup = New Button() With {.Text = "查询", .Location = New Point(550, 64), .Width = 90}
        lblMeterStatus = New Label() With {.Text = "复测3562：未连接", .Location = New Point(24, 110), .AutoSize = True, .ForeColor = Color.DarkOrange}

        Dim sourceGroup As New GroupBox() With {.Text = "原始NG记录", .Location = New Point(24, 145), .Size = New Size(790, 125)}
        lblSource = New Label() With {.Text = "请扫码查询电池历史记录。", .Location = New Point(18, 28), .AutoSize = False, .Size = New Size(750, 82)}
        sourceGroup.Controls.Add(lblSource)

        chkOverwrite = New CheckBox() With {.Text = "复测结果覆盖原数据", .Location = New Point(24, 300), .AutoSize = True, .Checked = False}
        btnRetest = New Button() With {.Text = "复测", .Location = New Point(230, 294), .Size = New Size(130, 38), .Enabled = False}
        lblResult = New Label() With {.Text = "复测结果：未测量", .Location = New Point(24, 355), .Font = New Font(Font, FontStyle.Bold), .AutoSize = True}

        Controls.AddRange(New Control() {title, barcodeLabel, txtBarcode, btnLookup, lblMeterStatus, sourceGroup, chkOverwrite, btnRetest, lblResult})
        AddHandler txtBarcode.KeyDown, AddressOf BarcodeKeyDown
        AddHandler btnLookup.Click, AddressOf LookupClick
        AddHandler btnRetest.Click, AddressOf RetestClick
        AddHandler FormClosing, AddressOf FormClosingHandler
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        txtBarcode.Focus()
        Try
            Dim portName As String = "COM" & (Initsetting.Retest3562Port + 1).ToString()
            meterService.Connect(portName, If(Initsetting.Retest3562Rate > 0, Initsetting.Retest3562Rate, 9600))
            lblMeterStatus.Text = "复测3562：已连接（" & portName & "）"
            lblMeterStatus.ForeColor = Color.DarkGreen
        Catch ex As Exception
            lblMeterStatus.Text = "复测3562：连接失败 - " & ex.Message
            lblMeterStatus.ForeColor = Color.Red
        End Try
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

        SetBusy(True)
        Try
            currentSource = Await Task.Run(Function() dataService.LoadByBatteryCode(code))
            lblSource.Text = String.Format("条码：{0}{1}原始结果：{2}    原始电压：{3:F2} mV    原始内阻：{4:F2} mΩ{1}托盘号：{5}    托盘位置：{6}",
                                           currentSource.BatteryCode, Environment.NewLine, currentSource.OriginalResult,
                                           currentSource.OriginalVoltage * 1000.0F, currentSource.OriginalResistance,
                                           currentSource.TrayCode, currentSource.TrayPosition)
            lblResult.Text = "复测结果：待测量"
            btnRetest.Enabled = meterService.IsConnected
        Catch ex As Exception
            currentSource = Nothing
            btnRetest.Enabled = False
            lblSource.Text = ex.Message
            MessageBox.Show(ex.Message, "NG复测查询失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Async Sub RetestClick(ByVal sender As Object, ByVal e As EventArgs)
        If isBusy OrElse currentSource Is Nothing Then Return
        If Not meterService.IsConnected Then
            MessageBox.Show("复测3562未连接，不能测量。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        SetBusy(True)
        Try
            Dim measurement As RetestMeasurement = Await Task.Run(Function() meterService.Measure())
            lblResult.Text = String.Format("复测结果：{0}    电压：{1:F2} mV    内阻：{2:F2} mΩ    时间：{3:yyyy-MM-dd HH:mm:ss}",
                                           measurement.Result, measurement.Voltage * 1000.0F, measurement.Resistance, measurement.MeasuredAt)
            lblResult.ForeColor = If(measurement.Result = "OK", Color.DarkGreen, Color.Red)

            Dim overwrite As Boolean = chkOverwrite.Checked
            If overwrite Then
                Dim confirm As DialogResult = MessageBox.Show("确认使用复测结果覆盖原始测试数据吗？" & Environment.NewLine &
                                                              "原始结果：" & currentSource.OriginalResult & "，复测结果：" & measurement.Result,
                                                              "确认覆盖", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                overwrite = (confirm = DialogResult.Yes)
            End If

            Dim request As New NgRetestSaveRequest() With {
                .Source = currentSource,
                .Measurement = measurement,
                .OverwriteOriginal = overwrite,
                .OperatorName = If(Initsetting.curOperator, operatorName),
                .MeterPort = meterService.PortName
            }
            Await Task.Run(Sub() dataService.SaveRetest(request))
            lblResult.Text &= If(overwrite, "    已覆盖原数据", "    未覆盖原数据")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "NG复测失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub SetBusy(ByVal busy As Boolean)
        isBusy = busy
        txtBarcode.Enabled = Not busy
        btnLookup.Enabled = Not busy
        chkOverwrite.Enabled = Not busy
        btnRetest.Enabled = Not busy AndAlso currentSource IsNot Nothing AndAlso meterService.IsConnected
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    Private Sub FormClosingHandler(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        meterService.Dispose()
    End Sub
End Class


