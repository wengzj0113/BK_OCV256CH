Friend Class FormalTestRunner
    Private ReadOnly middleMachine As SerialMiddleMachineClient
    Private ReadOnly channelPlanService As TestChannelPlanService
    Private ReadOnly resultMapper As TwoPassResultMapper
    Private ReadOnly applyResultsOnUi As Action(Of Action)

    Public Sub New()
        Me.New(New SerialMiddleMachineClient(Com_SCM), New TestChannelPlanService(), New TwoPassResultMapper())
    End Sub

    Public Sub New(traceLogger As Action(Of String))
        Me.New(New SerialMiddleMachineClient(Com_SCM, traceLogger), New TestChannelPlanService(), New TwoPassResultMapper())
    End Sub

    Public Sub New(middleMachineClient As SerialMiddleMachineClient,
                   planService As TestChannelPlanService,
                   mapper As TwoPassResultMapper,
                   Optional applyResultsOnUi As Action(Of Action) = Nothing)
        If middleMachineClient Is Nothing Then
            Throw New ArgumentNullException("middleMachineClient")
        End If
        If planService Is Nothing Then
            Throw New ArgumentNullException("planService")
        End If
        If mapper Is Nothing Then
            Throw New ArgumentNullException("mapper")
        End If

        middleMachine = middleMachineClient
        channelPlanService = planService
        resultMapper = mapper
        Me.applyResultsOnUi = applyResultsOnUi
    End Sub

    Public Function RunRangeMeasurement(startChannel As Integer,
                                        endChannel As Integer,
                                        Optional timeoutMs As Integer = 300000,
                                        Optional calibrationSnapshot As CalibrationSnapshot = Nothing) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildRangeSelection(startChannel, endChannel)
        RunTwoPassMeasurement(selection, timeoutMs, calibrationSnapshot:=calibrationSnapshot)
        Return selection
    End Function

    Public Function RunSparseMeasurement(channelNumbers As IEnumerable(Of Integer),
                                         Optional timeoutMs As Integer = 300000,
                                         Optional operationName As String = "正式测试",
                                         Optional cancellationRequested As Func(Of Boolean) = Nothing,
                                         Optional calibrationSnapshot As CalibrationSnapshot = Nothing) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildSparseSelection(channelNumbers)
        RunTwoPassMeasurement(selection, timeoutMs, operationName, cancellationRequested, calibrationSnapshot)
        Return selection
    End Function

    Public Function RunRetestMeasurement(retestFlags As Int16(),
                                         Optional timeoutMs As Integer = 300000,
                                         Optional calibrationSnapshot As CalibrationSnapshot = Nothing) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildRetestSelection(retestFlags)
        RunTwoPassMeasurement(selection, timeoutMs, calibrationSnapshot:=calibrationSnapshot)
        Return selection
    End Function

    Public Sub RunTwoPassMeasurement(selection As TestChannelSelection,
                                     Optional timeoutMs As Integer = 300000,
                                     Optional operationName As String = "正式测试",
                                     Optional cancellationRequested As Func(Of Boolean) = Nothing,
                                     Optional calibrationSnapshot As CalibrationSnapshot = Nothing)
        If selection Is Nothing Then
            Throw New ArgumentNullException("selection")
        End If

        Dim runCalibrationSnapshot As CalibrationSnapshot =
            If(calibrationSnapshot, CalibrationService.SharedInstance.CurrentSnapshot)
        Dim hiokiVoltages As List(Of Single) = Nothing
        Dim hiokiResistances As List(Of Single) = Nothing
        Dim dmmVoltages As List(Of Single) = Nothing
        Dim hasG1 As Boolean = selection.G1PhysicalChannels IsNot Nothing AndAlso selection.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = selection.G2PhysicalChannels IsNot Nothing AndAlso selection.G2PhysicalChannels.Count > 0
        Dim singleGroupTwoPass As Boolean = hasG1 Xor hasG2
        Dim hiokiReadCount As Integer = selection.ExpectedHiokiCount * If(singleGroupTwoPass, 2, 1)
        Dim testPlanDescription As String = String.Empty

        Try
            testPlanDescription = SerialMiddleMachineClient.DescribePlan(selection.ToMiddleMachinePlan())
            BusinessLogService.Write("操作", "成功", operationName & "开始", operationName, "已开始" & operationName & "。", testPlanDescription, "测试控制")
            Dim rateProfile As InstrumentRateProfile = InstrumentRateProfile.FromSettingName(Initsetting.ComRateMode)
            SerialMiddleMachineClient.LogMessage("Formal test start: " & testPlanDescription)
            SerialMiddleMachineClient.LogMessage("Calibration snapshot=" & runCalibrationSnapshot.Fingerprint &
                                                 ", source=" & runCalibrationSnapshot.SourceDescription)
            SerialMiddleMachineClient.LogMessage("Acquisition rate profile=" & rateProfile.SettingName &
                                                 ", controllerSpeed=" & rateProfile.ControllerSpeedValue.ToString() &
                                                 ", main 3562=" & If(String.IsNullOrWhiteSpace(Initsetting.Main3562SampleRate), "EXF", Initsetting.Main3562SampleRate) &
                                                 ", 34461A=" & If(String.IsNullOrWhiteSpace(Initsetting.Dmm34461Nplc), "0.2", Initsetting.Dmm34461Nplc) & " PLC")
            SerialMiddleMachineClient.LogMessage("Reset instruments before new test")
            ResetCachedData_3562()
            If Reset34461ForNewTest() <> 0 Then
                Throw New InvalidOperationException("34461 reset failed before test.")
            End If
            ClearInstrumentCaches()

            If PrepareCachedData_3562(selection.ExpectedHiokiCount) <> 0 Then
                Throw New InvalidOperationException("3562 failed to enter external trigger cache state.")
            End If
            If PrepareCachedData_34461(selection.ExpectedDmmCount) <> 0 Then
                Throw New InvalidOperationException("34461 failed to enter external trigger waiting state.")
            End If

            middleMachine.RunPlan(selection.ToMiddleMachinePlan(), timeoutMs, rateProfile.ControllerSpeedValue, cancellationRequested)

            If Trans_ReadCachedData_3562(hiokiReadCount, hiokiVoltages, hiokiResistances) <> 0 Then
                Throw New InvalidOperationException("3562 cached data read failed.")
            End If
            If singleGroupTwoPass Then
                '单组双遍时，3562 两遍都会产生缓存记录；第一遍是无电阻通路的占位记录，
                '第二遍才是实际内阻。只将第二遍映射到所选通道。
                hiokiResistances = hiokiResistances.GetRange(selection.ExpectedHiokiCount, selection.ExpectedHiokiCount)
                SerialMiddleMachineClient.LogMessage("3562 single-group two-pass cache: read=" & hiokiReadCount.ToString() &
                                                     ", mapped second pass=" & hiokiResistances.Count.ToString())
            End If
            If Trans_ReadCachedData_34461(selection.ExpectedDmmCount, dmmVoltages) <> 0 Then
                Throw New InvalidOperationException("34461 cached data read failed.")
            End If

            If cancellationRequested IsNot Nothing AndAlso cancellationRequested() Then
                Throw New OperationCanceledException("测试已由操作员停止。")
            End If
            Dim applyResults As Action = Sub()
                                             resultMapper.Apply(selection, hiokiResistances, dmmVoltages, runCalibrationSnapshot, RecieveData)
                                         End Sub
            If applyResultsOnUi Is Nothing Then
                applyResults()
            Else
                applyResultsOnUi(applyResults)
            End If
            SerialMiddleMachineClient.LogMessage("Formal test completed: " & SerialMiddleMachineClient.DescribePlan(selection.ToMiddleMachinePlan()))
            BusinessLogService.Write("操作", "成功", operationName & "测量完成", operationName, operationName & "测量阶段已完成。", testPlanDescription, "测试控制")
        Catch ex As Exception
            If TypeOf ex Is OperationCanceledException Then
                BusinessLogService.Write("操作", "已取消", operationName & "已停止", operationName, "操作员停止了当前测试。", testPlanDescription, "测试控制")
            Else
                BusinessLogService.Write("操作", "失败", operationName & "失败", operationName, operationName & "执行失败。", testPlanDescription & Environment.NewLine & ex.Message, "测试控制")
            End If
            Throw
        Finally
            ResetTestEnvironment()
        End Try
    End Sub

    Private Sub ResetTestEnvironment()
        TestEnvironmentResetService.ResetAll(middleMachine)
    End Sub

    Private Shared Sub ClearInstrumentCaches()
        Try
            ClearCachedData_3562()
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 clear serial buffer exception: " & ex.Message)
        End Try

        ClearCachedData_34461()
    End Sub
End Class
