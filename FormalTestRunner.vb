Friend Class FormalTestRunner
    Private ReadOnly middleMachine As SerialMiddleMachineClient
    Private ReadOnly channelPlanService As TestChannelPlanService
    Private ReadOnly resultMapper As TwoPassResultMapper

    Public Sub New()
        Me.New(New SerialMiddleMachineClient(Com_SCM), New TestChannelPlanService(), New TwoPassResultMapper())
    End Sub

    Public Sub New(middleMachineClient As SerialMiddleMachineClient,
                   planService As TestChannelPlanService,
                   mapper As TwoPassResultMapper)
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
    End Sub

    Public Function RunRangeMeasurement(startChannel As Integer,
                                        endChannel As Integer,
                                        Optional timeoutMs As Integer = 30000) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildRangeSelection(startChannel, endChannel)
        RunTwoPassMeasurement(selection, timeoutMs)
        Return selection
    End Function

    Public Function RunSparseMeasurement(channelNumbers As IEnumerable(Of Integer),
                                         Optional timeoutMs As Integer = 30000) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildSparseSelection(channelNumbers)
        RunTwoPassMeasurement(selection, timeoutMs)
        Return selection
    End Function

    Public Function RunRetestMeasurement(retestFlags As Int16(),
                                         Optional timeoutMs As Integer = 30000) As TestChannelSelection
        Dim selection As TestChannelSelection = channelPlanService.BuildRetestSelection(retestFlags)
        RunTwoPassMeasurement(selection, timeoutMs)
        Return selection
    End Function

    Public Sub RunTwoPassMeasurement(selection As TestChannelSelection,
                                     Optional timeoutMs As Integer = 30000)
        If selection Is Nothing Then
            Throw New ArgumentNullException("selection")
        End If

        Dim hiokiVoltages As List(Of Single) = Nothing
        Dim hiokiResistances As List(Of Single) = Nothing
        Dim dmmVoltages As List(Of Single) = Nothing

        Try
            SerialMiddleMachineClient.LogMessage("Formal test start: " & SerialMiddleMachineClient.DescribePlan(selection.ToMiddleMachinePlan()))
            ClearInstrumentCaches()

            If PrepareCachedData_3562(selection.ExpectedHiokiCount) <> 0 Then
                Throw New InvalidOperationException("3562 failed to enter external trigger cache state.")
            End If
            If PrepareCachedData_34461(selection.ExpectedDmmCount) <> 0 Then
                Throw New InvalidOperationException("34461 failed to enter external trigger waiting state.")
            End If

            middleMachine.RunPlan(selection.ToMiddleMachinePlan(), timeoutMs)

            If Trans_ReadCachedData_3562(selection.ExpectedHiokiCount, hiokiVoltages, hiokiResistances) <> 0 Then
                Throw New InvalidOperationException("3562 cached data read failed.")
            End If
            If Trans_ReadCachedData_34461(selection.ExpectedDmmCount, dmmVoltages) <> 0 Then
                Throw New InvalidOperationException("34461 cached data read failed.")
            End If

            resultMapper.Apply(selection, hiokiResistances, dmmVoltages, calibrateValueArray, RecieveData)
            SerialMiddleMachineClient.LogMessage("Formal test completed: " & SerialMiddleMachineClient.DescribePlan(selection.ToMiddleMachinePlan()))
        Catch
            Try
                middleMachine.StopAndClearLists()
            Catch
            End Try
            ClearInstrumentCaches()
            Throw
        End Try
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
