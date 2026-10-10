Imports System.IO
Imports System.Linq
Imports System.Diagnostics
Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions

Partial Public Class FrmManualTest

    Private ReadOnly channelButtons As New Dictionary(Of Integer, CheckBox)()
    Private ReadOnly selectedChannels As New HashSet(Of Integer)()
    Private isManualTesting As Boolean

    Public Sub New()
        InitializeComponent()
        PopulateChannelMatrix()
        UpdateSelectionSummary()
    End Sub

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

    Private Sub PopulateChannelMatrix()
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

            Dim deviceChannel As Integer = TrayChannelMapping.ToDeviceChannel(channel)
            Dim localChannel As Integer = If(deviceChannel <= 128, deviceChannel, deviceChannel - 128)
            If deviceChannel <= 128 Then
                channelToolTip.SetToolTip(box, "G1 CH" & localChannel.ToString())
            Else
                channelToolTip.SetToolTip(box, "G2 本地 CH" & localChannel.ToString())
            End If

            AddHandler box.CheckedChanged, AddressOf ChannelCheckedChanged
            channelButtons(channel) = box
            matrixPanel.Controls.Add(box, (channel - 1) Mod 16, (channel - 1) \ 16)
        Next
    End Sub

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
        If TrayChannelMapping.ToDeviceChannel(channel) <= 128 Then
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
        For deviceChannel As Integer = 1 To 128
            SetChannelChecked(TrayChannelMapping.ToStandardChannel(deviceChannel), True)
        Next
    End Sub

    Private Sub SelectG2Channels(sender As Object, e As EventArgs)
        For deviceChannel As Integer = 129 To 256
            SetChannelChecked(TrayChannelMapping.ToStandardChannel(deviceChannel), True)
        Next
    End Sub

    Private Sub UpdateSelectionSummary()
        Dim total As Integer = selectedChannels.Count
        Dim g1 As Integer = selectedChannels.Where(Function(channel) TrayChannelMapping.ToDeviceChannel(channel) <= 128).Count()
        Dim g2 As Integer = total - g1
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
            resultGrid.Rows.Clear()
            btnExport.Enabled = False
            SetTestingState(True, "测试中...")
            WriteManualLog("开始手动测试：" & String.Join(",", channels))

            Dim formalRunner As New FormalTestRunner(AddressOf WriteManualLog)
            formalRunner.RunSparseMeasurement(channels.Select(Function(channel) TrayChannelMapping.ToDeviceChannel(channel)), operationName:="手动测试")

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

    Private Sub StartMultipleManualTests(sender As Object, e As EventArgs)
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

        Dim repeatCount As Integer = Decimal.ToInt32(nudRepeatCount.Value)
        Dim outputDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testData")
        Dim outputPath As String = Path.Combine(outputDirectory,
                                                "ManualTest_Multi_" & DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") & ".csv")
        Dim writer As StreamWriter = Nothing
        Dim completedRounds As Integer = 0
        Dim currentRound As Integer = 0
        Dim failedRound As Integer = 0
        Dim failure As Exception = Nothing

        Try
            resultGrid.Rows.Clear()
            btnExport.Enabled = False
            SetTestingState(True, "准备多次测试...")
            WriteManualLog("开始手动多次测试：轮数=" & repeatCount.ToString(CultureInfo.InvariantCulture) &
                           "，通道=" & String.Join(",", channels))

            Directory.CreateDirectory(outputDirectory)
            writer = New StreamWriter(outputPath, False, New UTF8Encoding(True))
            writer.WriteLine("测试轮次,采集时间,通道,分组,电压 mV,内阻 mΩ,判定")
            writer.Flush()

            Dim formalRunner As New FormalTestRunner(AddressOf WriteManualLog)
            For currentRound = 1 To repeatCount
                lblStatus.Text = String.Format(CultureInfo.InvariantCulture, "第 {0}/{1} 次测试中...", currentRound, repeatCount)
                WriteManualLog(lblStatus.Text)

                Try
                    formalRunner.RunSparseMeasurement(channels.Select(Function(channel) TrayChannelMapping.ToDeviceChannel(channel)), operationName:="手动测试")
                    Dim capturedAt As DateTime = DateTime.Now
                    Dim csvRows As List(Of String()) = SnapshotManualTestRows(channels, currentRound, capturedAt)

                    For Each fields As String() In csvRows
                        writer.WriteLine(SerializeCsvRecord(fields))
                    Next
                    writer.Flush()

                    completedRounds = currentRound
                    DisplayManualTestResult(channels)
                    lblStatus.Text = String.Format(CultureInfo.InvariantCulture, "已完成 {0}/{1} 次", completedRounds, repeatCount)
                Catch ex As Exception
                    failure = ex
                    failedRound = currentRound
                    WriteManualLog("手动多次测试第 " & currentRound.ToString(CultureInfo.InvariantCulture) & " 次失败：" & ex.ToString())
                    Exit For
                End Try
            Next
        Catch ex As Exception
            failure = ex
            failedRound = currentRound
            WriteManualLog("手动多次测试初始化或保存失败：" & ex.ToString())
        Finally
            If writer IsNot Nothing Then
                Try
                    writer.Dispose()
                Catch ex As Exception
                    If failure Is Nothing Then
                        failure = ex
                        If failedRound = 0 Then failedRound = Math.Min(currentRound, repeatCount)
                    End If
                    WriteManualLog("关闭多次测试CSV失败：" & ex.ToString())
                End Try
            End If
            SetTestingState(False, lblStatus.Text)
        End Try

        If completedRounds > 0 Then
            Try
                Process.Start(New ProcessStartInfo(outputPath) With {.UseShellExecute = True})
            Catch openException As Exception
                WriteManualLog("多次测试CSV已保存但自动打开失败：" & openException.ToString())
                lblStatus.Text = "CSV已保存，自动打开失败"
                MsgBox("测试数据已保存，但自动打开CSV失败：" & openException.Message &
                       Environment.NewLine & outputPath,
                       vbOKOnly,
                       "CSV已保存")
                Return
            End Try
        End If

        If failure IsNot Nothing Then
            lblStatus.Text = If(completedRounds > 0,
                                String.Format(CultureInfo.InvariantCulture, "第 {0}/{1} 次失败，已保存 {2} 次", failedRound, repeatCount, completedRounds),
                                "多次测试失败")
            Dim failureDetail As String = If(failedRound > 0,
                                             "第 " & failedRound.ToString(CultureInfo.InvariantCulture) & " 次失败：",
                                             "多次测试失败：") & failure.Message
            If completedRounds > 0 Then
                failureDetail &= Environment.NewLine & "已完成的数据已保存并打开：" & outputPath
            ElseIf File.Exists(outputPath) Then
                failureDetail &= Environment.NewLine & "CSV未包含完整测试轮次，文件位置：" & outputPath
            End If
            MsgBox(failureDetail, vbOKOnly, "多次测试失败")
            Return
        End If

        lblStatus.Text = String.Format(CultureInfo.InvariantCulture, "多次测试完成：{0}/{1} 次", completedRounds, repeatCount)
        WriteManualLog("手动多次测试完成：轮数=" & completedRounds.ToString(CultureInfo.InvariantCulture) & "，CSV=" & outputPath)
        MsgBox("多次测试完成，数据已保存并打开：" & outputPath, vbOKOnly, "测试完成")
    End Sub

    Private Function SnapshotManualTestRows(channels As IEnumerable(Of Integer),
                                            roundNumber As Integer,
                                            capturedAt As DateTime) As List(Of String())
        Dim rows As New List(Of String())()
        Dim timestamp As String = capturedAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
        For Each channel As Integer In channels.OrderBy(Function(ch) ch)
            Dim index0 As Integer = channel - 1
            Dim voltage As Single = RecieveData(index0).Voltage
            Dim resistance As Single = RecieveData(index0).Resistance
            Dim deviceChannel As Integer = TrayChannelMapping.ToDeviceChannel(channel)
            Dim groupText As String = If(deviceChannel <= 128, "G1", "G2 本地CH" & (deviceChannel - 128).ToString(CultureInfo.InvariantCulture))
            Dim resultText As String = If(IsNG(voltage, resistance) = 0, "OK", "NG")
            rows.Add(New String() {
                roundNumber.ToString(CultureInfo.InvariantCulture),
                timestamp,
                "CH" & channel.ToString(CultureInfo.InvariantCulture),
                groupText,
                (voltage * 1000.0F).ToString("0.0000", CultureInfo.InvariantCulture),
                resistance.ToString("0.0000", CultureInfo.InvariantCulture),
                resultText
            })
        Next
        Return rows
    End Function

    Private Shared Function SerializeCsvRecord(fields As IEnumerable(Of String)) As String
        Return String.Join(",", fields.Select(Function(field) EscapeCsvField(field)).ToArray())
    End Function

    Private Sub SetTestingState(testing As Boolean, statusText As String)
        isManualTesting = testing
        isRuning = testing
        lblStatus.Text = statusText

        btnStart.Enabled = Not testing
        If btnMultiple IsNot Nothing Then btnMultiple.Enabled = Not testing
        If nudRepeatCount IsNot Nothing Then nudRepeatCount.Enabled = Not testing
        btnExport.Enabled = Not testing AndAlso resultGrid IsNot Nothing AndAlso resultGrid.Rows.Count > 0
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
            Dim deviceChannel As Integer = TrayChannelMapping.ToDeviceChannel(channel)
            Dim groupText As String = If(deviceChannel <= 128, "G1", "G2 本地CH" & (deviceChannel - 128).ToString())

            Dim rowIndex As Integer = resultGrid.Rows.Add("CH" & channel.ToString(),
                                                         groupText,
                                                         voltageMv.ToString("0.0000"),
                                                         resistance.ToString("0.0000"),
                                                         resultText)
            resultGrid.Rows(rowIndex).Tag = deviceChannel
        Next

        btnExport.Enabled = resultGrid.Rows.Count > 0
    End Sub

    Private Sub ExportManualTestResults(sender As Object, e As EventArgs)
        If resultGrid Is Nothing OrElse resultGrid.Rows.Count = 0 Then
            MsgBox("当前没有可导出的测试结果。", vbOKOnly, "导出结果")
            Return
        End If

        Dim defaultName As String = "ManualTest_" & DateTime.Now.ToString("yyyyMMdd_HHmmss")
        Dim requestedName As String = Microsoft.VisualBasic.Interaction.InputBox(
            "请输入CSV文件名（不需要输入扩展名）：",
            "导出测试结果",
            defaultName).Trim()
        If requestedName = String.Empty Then Return

        If requestedName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) Then
            requestedName = requestedName.Substring(0, requestedName.Length - 4)
        End If
        If requestedName = String.Empty OrElse requestedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
            MsgBox("文件名不能为空，也不能包含路径分隔符或系统不允许的字符。", vbOKOnly, "文件名无效")
            Return
        End If

        Try
            Dim outputDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testData")
            Directory.CreateDirectory(outputDirectory)
            Dim outputPath As String = Path.Combine(outputDirectory, requestedName & ".csv")
            If File.Exists(outputPath) AndAlso
               MsgBox("文件已存在，是否覆盖？" & Environment.NewLine & outputPath,
                      vbYesNo Or vbQuestion,
                      "确认覆盖") <> vbYes Then
                Return
            End If

            Dim csv As New StringBuilder()
            Dim headers As New List(Of String)()
            For Each column As DataGridViewColumn In resultGrid.Columns
                headers.Add(EscapeCsvField(column.HeaderText))
            Next
            headers.Add(EscapeCsvField("维修通道"))
            csv.AppendLine(String.Join(",", headers.ToArray()))

            For Each row As DataGridViewRow In resultGrid.Rows
                Dim fields As New List(Of String)()
                For Each cell As DataGridViewCell In row.Cells
                    fields.Add(EscapeCsvField(If(cell.Value Is Nothing, String.Empty, cell.Value.ToString())))
                Next
                If Not TypeOf row.Tag Is Integer Then Throw New InvalidOperationException("测试结果缺少对应的设备通道号。")
                fields.Add(CInt(row.Tag).ToString(CultureInfo.InvariantCulture))
                csv.AppendLine(String.Join(",", fields.ToArray()))
            Next

            File.WriteAllText(outputPath, csv.ToString(), New UTF8Encoding(True))
            lblStatus.Text = "结果已导出：" & Path.GetFileName(outputPath)
            WriteManualLog("导出测试结果：" & outputPath & "，行数=" & resultGrid.Rows.Count.ToString())
            Try
                Process.Start(New ProcessStartInfo(outputPath) With {.UseShellExecute = True})
            Catch openException As Exception
                WriteManualLog("CSV已保存但自动打开失败：" & openException.Message)
                MsgBox("文件已保存，但自动打开失败：" & openException.Message & Environment.NewLine & outputPath,
                       vbOKOnly,
                       "文件已保存")
                Return
            End Try

            MsgBox("测试结果已导出并打开：" & outputPath, vbOKOnly, "导出完成")
        Catch ex As Exception
            WriteManualLog("导出测试结果失败：" & ex.ToString())
            MsgBox("导出测试结果失败：" & ex.Message, vbOKOnly, "导出失败")
        End Try
    End Sub

    Private Shared Function EscapeCsvField(value As String) As String
        Dim text As String = If(value, String.Empty)
        Dim quote As String = ChrW(34).ToString()
        If text.Contains(",") OrElse text.Contains(quote) OrElse text.Contains(vbCr) OrElse text.Contains(vbLf) Then
            Return quote & text.Replace(quote, quote & quote) & quote
        End If
        Return text
    End Function

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
