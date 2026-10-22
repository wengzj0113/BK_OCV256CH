Imports System.Data
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks

Partial Public Class FrmManualTest
    Private preparedBarcodePlan As ManualTestBarcodePlan

    Private Sub TrayCodeChanged(sender As Object, e As EventArgs) Handles txtTrayCode.TextChanged
        preparedBarcodePlan = Nothing
        lblBarcodeStatus.Text = "请获取当前托盘的电芯条码"
    End Sub

    Private Sub TrayCodeKeyDown(sender As Object, e As KeyEventArgs) Handles txtTrayCode.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            e.SuppressKeyPress = True
            If btnGetBarcode.Enabled Then btnGetBarcode.PerformClick()
        End If
    End Sub

    Private Function CanStartManualOperation() As Boolean
        If isManualTesting OrElse isRuning OrElse plcAutomaticModeActive OrElse
           CalibrationService.IsCalibrationAcquisitionInProgress OrElse CalibrationService.IsBarcodeRetestInProgress Then
            MessageBox.Show(Me, "测试、自动流程或校准正在运行，请等待完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return AuthorizationService.RequireAuthenticatedUser(Me)
    End Function

    Private Async Sub GetManualBarcodes(sender As Object, e As EventArgs) Handles btnGetBarcode.Click
        If Not CanStartManualOperation() Then Return
        Dim tray As String = txtTrayCode.Text.Trim()
        If tray.Length = 0 OrElse tray.Length > 100 Then
            MessageBox.Show(Me, "请输入 1–100 个字符的托盘码。", "提示")
            txtTrayCode.Focus()
            Return
        End If
        preparedBarcodePlan = Nothing
        SetTestingState(True, "正在获取电芯条码...")
        Try
            Dim connection As String = OcvDataService.BuildConnectionString()
            Dim cells As DataTable = Await Task.Run(Function()
                                                       Using service As New OcvDataService(connection)
                                                           Return service.GetTrayCellsByTrayCode(tray)
                                                       End Using
                                                   End Function)
            preparedBarcodePlan = ManualTestBarcodePlan.Create(tray, cells, DateTime.Now)
            lblBarcodeStatus.Text = String.Format("托盘 {0}：{1} 个电芯码；来源：{2}", tray, preparedBarcodePlan.Codes.Count,
                                                  If(preparedBarcodePlan.IsSimulated, "模拟（只存本地）", "数据库"))
            For Each item In channelButtons
                Dim physical = TrayChannelMapping.ToDeviceChannel(item.Key)
                Dim code As String = Nothing
                preparedBarcodePlan.Codes.TryGetValue(item.Key, code)
                channelToolTip.SetToolTip(item.Value, "标准 CH" & item.Key.ToString() & " / 维修 CH" & physical.ToString() &
                                         Environment.NewLine & If(code, "没有电芯条码"))
            Next
            lblStatus.Text = If(preparedBarcodePlan.IsSimulated, "未查到条码，已生成测试条码", "条码已就绪，请选择通道并启动测试")
            WriteManualLog(lblBarcodeStatus.Text)
        Catch ex As Exception
            preparedBarcodePlan = Nothing
            lblBarcodeStatus.Text = "条码获取失败，请重试"
            lblStatus.Text = "获取条码失败"
            WriteManualLog("获取电芯条码失败：" & ex.Message)
            MessageBox.Show(Me, "获取条码失败：" & ex.Message, "获取条码", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            SetTestingState(False, lblStatus.Text)
        End Try
    End Sub

    Private Async Sub StartManualTest(sender As Object, e As EventArgs) Handles btnStart.Click
        Await ExecuteManualTestsAsync(1, False)
    End Sub

    Private Async Sub StartMultipleManualTests(sender As Object, e As EventArgs) Handles btnMultiple.Click
        Await ExecuteManualTestsAsync(Decimal.ToInt32(nudRepeatCount.Value), True)
    End Sub

    Private Async Function ExecuteManualTestsAsync(repeatCount As Integer, writeWorkbook As Boolean) As Task
        If Not CanStartManualOperation() Then Return
        Dim context As ManualTestRunContext
        Dim connectionString As String
        Try
            If preparedBarcodePlan Is Nothing OrElse Not String.Equals(preparedBarcodePlan.TrayCode, txtTrayCode.Text.Trim(), StringComparison.Ordinal) Then
                Throw New InvalidOperationException("请先获取当前托盘的电芯条码。")
            End If
            Dim channels = preparedBarcodePlan.ValidateChannels(GetSelectedChannels())
            If CheckCom_SCM() <> 1 Then Throw New InvalidOperationException("中位机串口通讯失败，无法启动手动测试。")
            context = New ManualTestRunContext(preparedBarcodePlan, channels, currentOCVNum, AuthorizationService.CurrentUserName, DateTime.Now,
                                              processNumSet, frmTest.hjwd, (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString(CultureInfo.InvariantCulture),
                                              FrmMesTask.chkResultDataUploadAssembly.Checked, Initsetting.MesApiAddress, Initsetting.EquipPC_ID, Initsetting.EquipPC_Password,
                                              Initsetting.downVoltage, Initsetting.upVoltage, Initsetting.downResistance, Initsetting.upResistance)
            connectionString = OcvDataService.BuildConnectionString()
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "启动前检查", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try

        Dim rounds As New List(Of ManualTestRoundSnapshot)()
        Dim workbook As ManualTestWorkbookWriter = Nothing
        Dim workbookRounds As Integer = 0
        Dim outputPath As String = Nothing
        Dim failure As Exception = Nothing
        SetTestingState(True, "准备手动测试...")
        Try
            resultGrid.Rows.Clear()
            btnExport.Enabled = False
            If writeWorkbook Then
                outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testData",
                                          "ManualTest_Multi_" & context.StartedAt.ToString("yyyyMMdd_HHmmss_fff") & ".xlsx")
                workbook = New ManualTestWorkbookWriter(outputPath, context.StartedAt)
            End If
            Dim runner As New FormalTestRunner(AddressOf WriteManualLog)
            Try
                For number As Integer = 1 To repeatCount
                    lblStatus.Text = String.Format("第 {0}/{1} 轮测试中...", number, repeatCount)
                    WriteManualLog(lblStatus.Text & "，通道=" & String.Join(",", context.Channels))
                    For Each channel As Integer In context.Channels
                        RecieveData(channel - 1).BatCode = context.Plan.Codes(channel)
                    Next
                    Dim startedAt As DateTime = DateTime.Now
                    runner.RunSparseMeasurement(context.Channels.Select(Function(c) TrayChannelMapping.ToDeviceChannel(c)), operationName:="手动测试")
                    Dim snapshot = ManualTestRoundSnapshot.Capture(context, number, RecieveData, startedAt, DateTime.Now)
                    rounds.Add(snapshot)
                    DisplayManualTestResult(snapshot)
                    If workbook IsNot Nothing Then
                        workbook.AddRound(number, snapshot.CapturedAt, SnapshotManualTestRows(snapshot))
                        workbookRounds += 1
                    End If
                Next
            Catch ex As Exception
                failure = ex
                WriteManualLog("手动测试或工作簿写入失败：" & ex.ToString())
            Finally
                If workbook IsNot Nothing Then
                    Try
                        workbook.Dispose()
                    Catch ex As Exception
                        If failure Is Nothing Then failure = ex
                        WriteManualLog("关闭工作簿失败：" & ex.Message)
                    End Try
                    workbook = Nothing
                End If
            End Try

            lblStatus.Text = If(failure Is Nothing, "测试完成", "测试已中止，已完成 " & rounds.Count.ToString() & " 轮")
            If failure IsNot Nothing Then
                MessageBox.Show(Me, "测试或工作簿处理未全部完成：" & failure.Message & Environment.NewLine &
                                "有效测量轮数：" & rounds.Count.ToString(), "手动测试", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            If rounds.Count > 0 Then Await ConfirmAndSaveManualResultsAsync(rounds, connectionString)
            If failure IsNot Nothing Then lblStatus.Text = "测试中止（已完成 " & rounds.Count.ToString() & " 轮）；" & lblStatus.Text

            If workbookRounds > 0 Then
                Try
                    Process.Start(New ProcessStartInfo(outputPath) With {.UseShellExecute = True})
                Catch ex As Exception
                    WriteManualLog("工作簿已保存但自动打开失败：" & ex.Message)
                    MessageBox.Show(Me, "工作簿已保存，但自动打开失败：" & ex.Message & Environment.NewLine & outputPath, "工作簿已保存")
                End Try
            End If
        Catch ex As Exception
            lblStatus.Text = "手动测试处理失败"
            WriteManualLog("手动测试处理失败：" & ex.ToString())
            MessageBox.Show(Me, ex.Message, "手动测试", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If workbook IsNot Nothing Then
                Try
                    workbook.Dispose()
                Catch ex As Exception
                    WriteManualLog("清理工作簿失败：" & ex.Message)
                End Try
            End If
            SetTestingState(False, lblStatus.Text)
        End Try
    End Function

    Private Async Function ConfirmAndSaveManualResultsAsync(rounds As List(Of ManualTestRoundSnapshot), connectionString As String) As Task
        Dim context = rounds(0).Context
        Dim detail As String = If(context.Plan.IsSimulated, "模拟条码结果只保存到本地 OCV_DATA。", "本地保存成功后，按 MES 配置上传最后一轮。")
        If MessageBox.Show(Me, "已完成 " & rounds.Count.ToString() & " 轮测量，是否保存到数据库？" & Environment.NewLine &
                           "同一电芯条码更新原结果，最终保留最后一轮。" & Environment.NewLine & detail,
                           "保存测试结果", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            lblStatus.Text = "测量完成，未保存到数据库"
            Return
        End If
        Try
            lblStatus.Text = "正在保存测试结果到 OCV_DATA..."
            Await Task.Run(Sub() ManualTestResultService.SaveRounds(rounds, connectionString))
        Catch ex As Exception
            lblStatus.Text = "本地保存失败；未提交 MES"
            WriteManualLog(ex.ToString())
            MessageBox.Show(Me, ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        lblStatus.Text = "本地结果已保存"
        If context.Plan.IsSimulated Then
            lblStatus.Text &= "；模拟数据不上传 MES"
        ElseIf Not context.MesEnabled OrElse String.IsNullOrWhiteSpace(context.MesAddress) Then
            lblStatus.Text &= "；MES 结果上传未启用或未配置"
        Else
            Try
                lblStatus.Text = "本地已保存，正在上传最后一轮到 MES..."
                Dim uploader As New ResultDataUploadAssembly(FrmMesTask.listBoxOutput, rounds.Last())
                lblStatus.Text = "本地已保存；" & Await uploader.UploadSnapshotAsync()
            Catch ex As Exception
                lblStatus.Text = "本地已保存；MES 上传或待上传队列写入失败"
                WriteManualLog("MES 上传失败：" & ex.Message)
                MessageBox.Show(Me, "本地测量结果已保存，但 MES 处理失败：" & ex.Message, "MES 上传", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End If
        WriteManualLog(lblStatus.Text)
    End Function

    Private Function SnapshotManualTestRows(snapshot As ManualTestRoundSnapshot) As List(Of ManualTestWorkbookRow)
        Dim data = snapshot.CopyMeasurements()
        Return snapshot.Context.Channels.Select(Function(channel)
                                                     Dim item = data(channel - 1)
                                                     Dim device = TrayChannelMapping.ToDeviceChannel(channel)
                                                     Return New ManualTestWorkbookRow With {
                                                         .ChannelNumber = channel, .BatteryCode = item.BatCode,
                                                         .GroupText = If(device <= 128, "G1", "G2 本地CH" & (device - 128).ToString()),
                                                         .VoltageMillivolts = CDbl(item.Voltage) * 1000.0R,
                                                         .ResistanceMilliohms = item.Resistance, .ResultText = item.Result}
                                                 End Function).ToList()
    End Function

    Private Sub DisplayManualTestResult(snapshot As ManualTestRoundSnapshot)
        resultGrid.Rows.Clear()
        For Each item As ManualTestWorkbookRow In SnapshotManualTestRows(snapshot)
            Dim index = resultGrid.Rows.Add("CH" & item.ChannelNumber.ToString(), item.GroupText, item.BatteryCode,
                                            item.VoltageMillivolts.ToString("0.0000"), item.ResistanceMilliohms.ToString("0.0000"), item.ResultText)
            resultGrid.Rows(index).Tag = TrayChannelMapping.ToDeviceChannel(item.ChannelNumber)
        Next
        btnExport.Enabled = Not isManualTesting AndAlso resultGrid.Rows.Count > 0
    End Sub
End Class
