Imports System.IO.Ports
Imports System.Threading
Imports System.Windows.Forms

Public Class ScmTimingSettings
    Public Const DefaultRelaySettleMs As Integer = 5
    Public Const DefaultChannelSettleMs As Integer = 50
    Public Const DefaultAutoStepMs As Integer = 10
    Public Const DefaultTriggerPulseMs As Integer = 1
    Public Const DefaultPhaseGuardMs As Integer = 100
    Public Const MinRelaySettleMs As Integer = 5
    Public Const MaxRelaySettleMs As Integer = 5000
    Public Const MinChannelSettleMs As Integer = 0
    Public Const MaxChannelSettleMs As Integer = 5000
    Public Const MinAutoStepMs As Integer = 10
    Public Const MaxAutoStepMs As Integer = 10000
    Public Const MinTriggerPulseMs As Integer = 1
    Public Const MaxTriggerPulseMs As Integer = 1000
    Public Const MinPhaseGuardMs As Integer = 100
    Public Const MaxPhaseGuardMs As Integer = 10000

    Public Property RelaySettleMs As Integer
    Public Property ChannelSettleMs As Integer
    Public Property AutoStepMs As Integer
    Public Property TriggerPulseMs As Integer
    Public Property PhaseGuardMs As Integer

    Public Sub New()
        RelaySettleMs = DefaultRelaySettleMs
        ChannelSettleMs = DefaultChannelSettleMs
        AutoStepMs = DefaultAutoStepMs
        TriggerPulseMs = DefaultTriggerPulseMs
        PhaseGuardMs = DefaultPhaseGuardMs
    End Sub

    Public Sub New(relaySettleMs As Integer,
                   channelSettleMs As Integer,
                   autoStepMs As Integer,
                   triggerPulseMs As Integer,
                   phaseGuardMs As Integer)
        Me.RelaySettleMs = relaySettleMs
        Me.ChannelSettleMs = channelSettleMs
        Me.AutoStepMs = autoStepMs
        Me.TriggerPulseMs = triggerPulseMs
        Me.PhaseGuardMs = phaseGuardMs
    End Sub

    Public Function Validate() As String
        Dim errorMessage As String = ValidateRange("继电器稳定时间", RelaySettleMs, MinRelaySettleMs, MaxRelaySettleMs)
        If errorMessage IsNot Nothing Then Return errorMessage

        errorMessage = ValidateRange("通道稳定时间", ChannelSettleMs, MinChannelSettleMs, MaxChannelSettleMs)
        If errorMessage IsNot Nothing Then Return errorMessage

        errorMessage = ValidateRange("仪表等待时间", AutoStepMs, MinAutoStepMs, MaxAutoStepMs)
        If errorMessage IsNot Nothing Then Return errorMessage

        errorMessage = ValidateRange("触发脉宽时间", TriggerPulseMs, MinTriggerPulseMs, MaxTriggerPulseMs)
        If errorMessage IsNot Nothing Then Return errorMessage

        Return ValidateRange("遍末保护时间", PhaseGuardMs, MinPhaseGuardMs, MaxPhaseGuardMs)
    End Function

    Private Shared Function ValidateRange(parameterName As String,
                                           value As Integer,
                                           minimum As Integer,
                                           maximum As Integer) As String
        If value < minimum OrElse value > maximum Then
            Return String.Format("{0}必须在{1}-{2} ms范围内，当前值为{3}。", parameterName, minimum, maximum, value)
        End If
        Return Nothing
    End Function
End Class

Public Class ScmTimingWriteException
    Inherits InvalidOperationException

    Public ReadOnly Property ParameterName As String
    Public ReadOnly Property RegisterAddress As UShort
    Public ReadOnly Property Value As Integer
    Public ReadOnly Property CompletedCount As Integer

    Public Sub New(parameterName As String,
                   registerAddress As UShort,
                   value As Integer,
                   completedCount As Integer,
                   innerException As Exception)
        MyBase.New(String.Format("SCM parameter {0} (0x{1:X4}) write value {2} failed; {3} items completed: {4}", parameterName, registerAddress, value, completedCount, innerException.Message), innerException)
        Me.ParameterName = parameterName
        Me.RegisterAddress = registerAddress
        Me.Value = value
        Me.CompletedCount = completedCount
    End Sub
End Class

Public Class MiddleMachineChannelPlan
    Public Property SelectedPhysicalChannels As List(Of Integer)
    Public Property G1PhysicalChannels As List(Of Integer)
    Public Property G2PhysicalChannels As List(Of Integer)
    Public Property G2LocalChannels As List(Of Integer)
    Public Property ExecutionOrder As List(Of Integer)
    Public Property ExpectedHiokiCount As Integer
    Public Property ExpectedDmmCount As Integer

    Public Sub New()
        SelectedPhysicalChannels = New List(Of Integer)()
        G1PhysicalChannels = New List(Of Integer)()
        G2PhysicalChannels = New List(Of Integer)()
        G2LocalChannels = New List(Of Integer)()
        ExecutionOrder = New List(Of Integer)()
    End Sub
End Class

Public Class SerialMiddleMachineClient
    Private Const FrameHead As Byte = &HAA
    Private Const CommandRead As Byte = &H3
    Private Const CommandWrite As Byte = &H6
    Private Const CommandEvent As Byte = &H41
    Private Const StatusEventRegister As UShort = &H40
    Private Const StatusCompleted As UShort = &H6
    Private Const StatusError As UShort = &H64
    Private Const DefaultTimeoutMs As Integer = 30000
    Private Const DefaultTestTimeoutMs As Integer = 300000
    Private Const WriteResponseTimeoutMs As Integer = 3000
    Private Const WriteRetryCount As Integer = 3
    Private Const WriteRetryDelayMs As Integer = 150
    Private Const StopSettleDelayMs As Integer = 250
    Private Const ScmConfigWriteIntervalMs As Integer = 100

    Private Const RegStartStop As UShort = &H30
    Private Const RegG1Start As UShort = &H31
    Private Const RegG1End As UShort = &H32
    Private Const RegRelaySettleMs As UShort = &H33
    Private Const RegChannelSettleMs As UShort = &H34
    Private Const RegTwoPass As UShort = &H35
    Private Const RegAutoStepMs As UShort = &H36
    Private Const RegTriggerPulseMs As UShort = &H37
    Private Const RegSpeed As UShort = &H38
    Private Const RegG1PlanMode As UShort = &H60
    Private Const RegG1ListCount As UShort = &H61
    Private Const RegG1ListClear As UShort = &H62
    Private Const RegSyncMode As UShort = &H63
    Private Const RegG2PlanMode As UShort = &H64
    Private Const RegG2ListCount As UShort = &H65
    Private Const RegG2ListClear As UShort = &H66
    Private Const RegG2Start As UShort = &H67
    Private Const RegG2End As UShort = &H68
    Private Const RegGroupEnable As UShort = &H6A
    Private Const RegMeasureItems As UShort = &H6B
    Private Const RegPhaseGuardMs As UShort = &H6C
    Private Const RegG1ListBase As UShort = &H70
    Private Const RegG2ListBase As UShort = &H100
    Private Const RegWriteResultStart As UShort = &H5B
    Private Const RegBasicStatusStart As UShort = &H40
    Private Const RegBasicStatusCount As UShort = 7

    Private ReadOnly serialPort As SerialPort
    Public Property TraceLogger As Action(Of String)

    Public Shared Sub LogMessage(message As String)
        AppLog.Write("MiddleMachine", message)
    End Sub

    Private Sub TraceMessage(message As String)
        LogMessage(message)
        Try
            If TraceLogger IsNot Nothing Then TraceLogger.Invoke("控制板协议 " & message)
        Catch
        End Try
    End Sub

    Public Sub New(port As SerialPort, Optional traceLogger As Action(Of String) = Nothing)
        If port Is Nothing Then
            Throw New ArgumentNullException("port")
        End If

        serialPort = port
        TraceLogger = traceLogger
        ConfigurePort()
    End Sub

    Public Sub ConfigurePort()
        If String.IsNullOrWhiteSpace(serialPort.PortName) Then
            serialPort.PortName = "COM" & (Initsetting.SCMComPort + 1).ToString()
        End If

        serialPort.BaudRate = DefaultScmBaudRate
        serialPort.Parity = Parity.None
        serialPort.DataBits = 8
        serialPort.StopBits = StopBits.One
        serialPort.ReadTimeout = 1000
        serialPort.WriteTimeout = 1000
        TraceMessage(String.Format("Serial configured: {0}, {1}, 8N1", serialPort.PortName, serialPort.BaudRate))
    End Sub

    Public Sub EnsureOpen()
        ConfigurePort()
        If Not serialPort.IsOpen Then
            serialPort.Open()
        End If
    End Sub

    Public Sub StopTest()
        WriteRegister(RegStartStop, &H0)
    End Sub

    Public Sub ResetToDefaults()
        WriteRegister(RegStartStop, &H6)
    End Sub

    Public Sub StopAndClearLists()
        Try
            StopTest()
            Thread.Sleep(StopSettleDelayMs)
        Finally
            Try
                ClearChannelLists()
            Catch ex As Exception
                TraceMessage("ClearChannelLists failed after STOP: " & ex.Message)
            End Try
        End Try
    End Sub

    Public Sub ClearChannelLists()
        Dim firstException As Exception = Nothing
        Try
            WriteRegister(RegG1ListClear, &H1)
        Catch ex As Exception
            firstException = ex
            TraceMessage("Clear G1 list failed: " & ex.Message)
        End Try
        Try
            WriteRegister(RegG2ListClear, &H1)
        Catch ex As Exception
            If firstException Is Nothing Then firstException = ex
            TraceMessage("Clear G2 list failed: " & ex.Message)
        End Try
        If firstException IsNot Nothing Then Throw firstException
    End Sub

    Public Sub ResetToSafeState(Optional totalTimeoutMs As Integer = 3000)
        TraceMessage("Middle machine safe reset start")
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(1, totalTimeoutMs))

        TryWriteRegisterForDeadline(RegStartStop, &H0, deadline)
        Dim remainingTimeoutMs As Integer = GetRemainingTimeoutMs(deadline)
        If remainingTimeoutMs > 0 Then
            Thread.Sleep(Math.Min(StopSettleDelayMs, remainingTimeoutMs))
        End If

        TryWriteRegisterForDeadline(RegG1ListClear, &H1, deadline)
        TryWriteRegisterForDeadline(RegG2ListClear, &H1, deadline)

        Dim safeWrites As UShort(,) = {
            {RegSyncMode, &H0},
            {RegGroupEnable, &H0},
            {RegTwoPass, &H0},
            {RegMeasureItems, &H0},
            {RegSpeed, &H0}
        }
        For i As Integer = 0 To safeWrites.GetLength(0) - 1
            If GetRemainingTimeoutMs(deadline) <= 0 Then Exit For
            TryWriteRegisterForDeadline(safeWrites(i, 0), safeWrites(i, 1), deadline)
        Next
        TraceMessage("Middle machine safe reset completed")
    End Sub

    Public Sub StartTest()
        WriteRegister(RegStartStop, &H1)
    End Sub

    Public Function RunPlan(plan As MiddleMachineChannelPlan,
                             Optional timeoutMs As Integer = DefaultTestTimeoutMs,
                             Optional speedValue As UShort = 3,
                             Optional cancellationRequested As Func(Of Boolean) = Nothing) As UShort
        TraceMessage("RunPlan: " & DescribePlan(plan))
        ThrowIfCancellationRequested(cancellationRequested)
        EnsureOpen()
        serialPort.DiscardInBuffer()
        ConfigurePlan(plan, speedValue)
        ThrowIfCancellationRequested(cancellationRequested)
        StartTest()
        Return WaitForCompletion(timeoutMs, cancellationRequested)
    End Function

    Public Sub ConfigurePlan(plan As MiddleMachineChannelPlan, speedValue As UShort)
        If plan Is Nothing OrElse plan.ExecutionOrder Is Nothing OrElse plan.ExecutionOrder.Count = 0 Then
            Throw New InvalidOperationException("Channel plan is empty.")
        End If

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0
        If Not hasG1 AndAlso Not hasG2 Then
            Throw New InvalidOperationException("Channel plan has no enabled group.")
        End If

        If speedValue > 3 Then
            Throw New ArgumentOutOfRangeException("speedValue", "Speed value must be between 0 and 3.")
        End If

        ResetToDefaults()
        WriteRegister(RegSpeed, speedValue)

        '正式测试始终需要两遍：分别采集内阻和电压。
        '单组时两遍由同一组执行，3562 会缓存两遍记录；读取层按实际内阻遍次回填。
        Dim twoPassValue As UShort = CUShort(0)
        WriteRegister(RegTwoPass, twoPassValue)

        Dim synchronizedPlan As Boolean = IsSynchronizedPlan(plan)
        If synchronizedPlan Then
            WriteRegister(RegSyncMode, &H0)
            ConfigureG1Channels(plan.G1PhysicalChannels)
        Else
            WriteRegister(RegSyncMode, &H1)
            If hasG1 Then
                ConfigureG1Channels(plan.G1PhysicalChannels)
            End If
            If hasG2 Then
                ConfigureG2Channels(ToG2LocalChannels(plan.G2PhysicalChannels))
            End If
        End If

        WriteRegister(RegGroupEnable, GetGroupEnableValue(hasG1, hasG2))
        VerifyPlan(plan, speedValue)
    End Sub

    Public Sub ConfigureSingleRange(startChannel As Integer, endChannel As Integer)
        Dim plan As New MiddleMachineChannelPlan()
        For channel As Integer = startChannel To endChannel
            ValidateChannel(channel)
            If channel <= 128 Then
                plan.G1PhysicalChannels.Add(channel)
            Else
                plan.G2PhysicalChannels.Add(channel)
            End If
            plan.ExecutionOrder.Add(channel)
        Next
        ConfigurePlan(plan, CUShort(3))
    End Sub

    Public Sub ConfigureChannelList(channelNumbers As IList(Of Integer))
        Dim plan As New MiddleMachineChannelPlan()
        For Each channel As Integer In channelNumbers
            ValidateChannel(channel)
            If channel <= 128 Then
                plan.G1PhysicalChannels.Add(channel)
            Else
                plan.G2PhysicalChannels.Add(channel)
            End If
            plan.ExecutionOrder.Add(channel)
        Next
        ConfigurePlan(plan, CUShort(3))
    End Sub

    Public Sub ConfigureTwoRanges(firstStartChannel As Integer,
                                  firstEndChannel As Integer,
                                  secondStartChannel As Integer,
                                  secondEndChannel As Integer)
        Dim plan As New MiddleMachineChannelPlan()
        For channel As Integer = firstStartChannel To firstEndChannel
            ValidateChannel(channel)
            plan.G1PhysicalChannels.Add(channel)
            plan.ExecutionOrder.Add(channel)
        Next
        For localChannel As Integer = secondStartChannel To secondEndChannel
            ValidateLocalChannel(localChannel)
            plan.G2PhysicalChannels.Add(localChannel + 128)
            plan.ExecutionOrder.Add(localChannel + 128)
        Next
        ConfigurePlan(plan, CUShort(3))
    End Sub

    Public Function RunSingleRange(startChannel As Integer, endChannel As Integer, Optional timeoutMs As Integer = DefaultTestTimeoutMs) As UShort
        ConfigureSingleRange(startChannel, endChannel)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Function RunChannelList(channelNumbers As IList(Of Integer), Optional timeoutMs As Integer = DefaultTestTimeoutMs) As UShort
        ConfigureChannelList(channelNumbers)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Function RunTwoRanges(firstStartChannel As Integer,
                                 firstEndChannel As Integer,
                                 secondStartChannel As Integer,
                                 secondEndChannel As Integer,
                                 Optional timeoutMs As Integer = DefaultTestTimeoutMs) As UShort
        ConfigureTwoRanges(firstStartChannel, firstEndChannel, secondStartChannel, secondEndChannel)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Sub WriteRegister(address As UShort, value As UShort)
        WriteRegister(address, value, False)
    End Sub

    Public Sub WriteRegisterAndVerifyStatus(address As UShort, value As UShort)
        WriteRegister(address, value, True)
    End Sub

    Public Function ReadScmTimingSettings() As ScmTimingSettings
        Dim timingValues As UShort() = ReadRegisters(RegRelaySettleMs, 5)
        Dim phaseGuardValues As UShort() = ReadRegisters(RegPhaseGuardMs, 1)

        If timingValues Is Nothing OrElse timingValues.Length < 5 OrElse
           phaseGuardValues Is Nothing OrElse phaseGuardValues.Length < 1 Then
            Throw New InvalidOperationException("SCM时间参数读取响应不完整。")
        End If

        Return New ScmTimingSettings(
            CInt(timingValues(0)),
            CInt(timingValues(1)),
            CInt(timingValues(3)),
            CInt(timingValues(4)),
            CInt(phaseGuardValues(0)))
    End Function

    Public Sub WriteScmTimingSettings(settings As ScmTimingSettings)
        If settings Is Nothing Then
            Throw New ArgumentNullException("settings")
        End If

        Dim validationError As String = settings.Validate()
        If Not String.IsNullOrWhiteSpace(validationError) Then
            Throw New ArgumentException(validationError, "settings")
        End If

        EnsureScmTimingSettingsWritable()

        Dim writes As ScmTimingRegister() = {
            New ScmTimingRegister("继电器稳定时间", RegRelaySettleMs, settings.RelaySettleMs),
            New ScmTimingRegister("通道稳定时间", RegChannelSettleMs, settings.ChannelSettleMs),
            New ScmTimingRegister("仪表等待时间", RegAutoStepMs, settings.AutoStepMs),
            New ScmTimingRegister("触发脉宽时间", RegTriggerPulseMs, settings.TriggerPulseMs),
            New ScmTimingRegister("遍末保护时间", RegPhaseGuardMs, settings.PhaseGuardMs)
        }

        Dim completedCount As Integer = 0
        For Each item As ScmTimingRegister In writes
            Try
                WriteRegisterAndVerifyStatus(item.Address, CUShort(item.Value))
            Catch ex As Exception
                Throw New ScmTimingWriteException(item.ParameterName,
                                                  item.Address,
                                                  item.Value,
                                                  completedCount,
                                                  ex)
            End Try

            completedCount += 1
            If completedCount < writes.Length Then
                Thread.Sleep(ScmConfigWriteIntervalMs)
            End If
        Next
    End Sub

    Private Sub EnsureScmTimingSettingsWritable()
        Dim values As UShort() = ReadRegisters(RegBasicStatusStart, 1)
        If values Is Nothing OrElse values.Length = 0 Then
            Throw New InvalidOperationException("无法读取 SCM 当前状态，未执行参数写入。")
        End If

        Dim status As UShort = values(0)
        If status <> 0 AndAlso status <> StatusCompleted AndAlso status <> StatusError Then
            Throw New InvalidOperationException(String.Format(
                "SCM当前状态为{0}(0x{1:X4})，参数已锁定，请先停止测试。",
                GetScmStatusName(status),
                status))
        End If
    End Sub

    Private Shared Function GetScmStatusName(status As UShort) As String
        Select Case status
            Case 0 : Return "TEST_IDLE"
            Case 1 : Return "TEST_WAIT_RELAY_SETTLE"
            Case 2 : Return "TEST_WAIT_CHANNEL_SETTLE"
            Case 3 : Return "TEST_WAIT_INSTRUMENT"
            Case 4 : Return "TEST_WAIT_PHASE_GUARD"
            Case 6 : Return "TEST_DONE"
            Case 7 : Return "TEST_PAUSE"
            Case 100 : Return "TEST_ERROR"
            Case Else : Return "UNKNOWN"
        End Select
    End Function

    Private Structure ScmTimingRegister
        Public ReadOnly ParameterName As String
        Public ReadOnly Address As UShort
        Public ReadOnly Value As Integer

        Public Sub New(parameterName As String, address As UShort, value As Integer)
            Me.ParameterName = parameterName
            Me.Address = address
            Me.Value = value
        End Sub
    End Structure

    Private Sub WriteRegister(address As UShort,
                               value As UShort,
                               verifyWriteStatus As Boolean,
                               Optional responseTimeoutMs As Integer = WriteResponseTimeoutMs,
                               Optional retryCount As Integer = WriteRetryCount)
        Dim request As Byte() = BuildWriteFrame(address, value)
        Dim lastException As Exception = Nothing
        For attempt As Integer = 1 To Math.Max(1, retryCount)
            Try
                EnsureOpen()
                serialPort.DiscardInBuffer()
                TraceMessage(String.Format("TX WRITE 0x{0:X4}=0x{1:X4} attempt={2}/{3}: {4}", address, value, attempt, WriteRetryCount, BytesToHex(request)))
                serialPort.Write(request, 0, request.Length)

                Dim echo As Byte() = ReadSpecificResponse(CommandWrite, 8, Math.Max(1, responseTimeoutMs))
                If echo Is Nothing Then
                    TraceMessage(String.Format("RX WRITE TIMEOUT 0x{0:X4}=0x{1:X4} attempt={2}", address, value, attempt))
                    Throw New TimeoutException(String.Format("Middle machine write response timeout: 0x{0:X4}=0x{1:X4}.", address, value))
                End If
                TraceMessage("RX WRITE ECHO: " & BytesToHex(echo))
                If Not BytesEqual(request, echo) Then
                    Throw New InvalidOperationException(String.Format("Middle machine write echo mismatch: 0x{0:X4}=0x{1:X4}.", address, value))
                End If

                If verifyWriteStatus Then
                    VerifyLastWriteStatus(address)
                End If
                Return
            Catch ex As Exception
                lastException = ex
                If attempt < Math.Max(1, retryCount) Then
                    TraceMessage(String.Format("WRITE retry scheduled 0x{0:X4}=0x{1:X4} after: {2}", address, value, ex.Message))
                    Thread.Sleep(WriteRetryDelayMs)
                End If
            End Try
        Next
        Throw lastException
    End Sub

    Private Sub TryWriteRegisterForDeadline(address As UShort, value As UShort, deadline As DateTime)
        Dim remainingTimeoutMs As Integer = GetRemainingTimeoutMs(deadline)
        If remainingTimeoutMs <= 0 Then Return

        Try
            WriteRegister(address,
                          value,
                          False,
                          Math.Min(remainingTimeoutMs, WriteResponseTimeoutMs),
                          1)
        Catch ex As Exception
            TraceMessage(String.Format("Middle machine bounded reset write failed: 0x{0:X4}=0x{1:X4}: {2}", address, value, ex.Message))
        End Try
    End Sub

    Private Shared Function GetRemainingTimeoutMs(deadline As DateTime) As Integer
        Return CInt(Math.Max(0, Math.Ceiling((deadline - DateTime.UtcNow).TotalMilliseconds)))
    End Function

    Public Function ReadWriteStatusRegisters() As UShort()
        Return ReadRegisters(RegWriteResultStart, 3)
    End Function

    Public Sub VerifyLastWriteStatus(expectedAddress As UShort)
        Dim values As UShort() = ReadWriteStatusRegisters()
        If values Is Nothing OrElse values.Length < 3 Then
            Throw New InvalidOperationException("Middle machine write status register response is incomplete.")
        End If
        If values(0) <> 0 Then
            Throw New InvalidOperationException(String.Format("Middle machine write failed. result=0x{0:X4}, address=0x{1:X4}, lock=0x{2:X4}.", values(0), values(1), values(2)))
        End If
        If values(1) <> expectedAddress Then
            Throw New InvalidOperationException(String.Format("Middle machine last write address mismatch. expected=0x{0:X4}, actual=0x{1:X4}.", expectedAddress, values(1)))
        End If
    End Sub

    Public Function ReadBasicStatusRegisters() As UShort()
        Return ReadRegisters(RegBasicStatusStart, RegBasicStatusCount)
    End Function

    ''' <summary>
    ''' 通过只读状态寄存器确认控制板正在响应 AA 协议。
    ''' </summary>
    Public Function CheckConnection(Optional timeoutMs As Integer = 3000) As Boolean
        Try
            Dim values As UShort() = ReadRegisters(RegBasicStatusStart, RegBasicStatusCount, timeoutMs)
            Return values IsNot Nothing AndAlso values.Length = RegBasicStatusCount
        Catch ex As Exception
            TraceMessage("CheckConnection failed: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadRegisters(startAddress As UShort, count As UShort, Optional timeoutMs As Integer = DefaultTimeoutMs) As UShort()
        If count = 0 OrElse count > 24 Then
            Throw New ArgumentOutOfRangeException("count")
        End If

        EnsureOpen()
        Dim request As Byte() = BuildReadFrame(startAddress, count)
        serialPort.DiscardInBuffer()
        TraceMessage(String.Format("TX READ 0x{0:X4}, count={1}: {2}", startAddress, count, BytesToHex(request)))
        serialPort.Write(request, 0, request.Length)

        Dim responseLength As Integer = 3 + count * 2 + 2
        Dim response As Byte() = ReadSpecificResponse(CommandRead, responseLength, timeoutMs)
        If response Is Nothing Then TraceMessage(String.Format("RX READ TIMEOUT 0x{0:X4}, count={1}", startAddress, count))
        TraceMessage("RX READ: " & BytesToHex(response))
        If response Is Nothing Then
            Throw New TimeoutException("Timed out reading middle machine registers.")
        End If
        If response(2) <> CByte(count * 2) Then
            Throw New InvalidOperationException("Middle machine read byte count mismatch.")
        End If

        Dim values(count - 1) As UShort
        For i As Integer = 0 To count - 1
            values(i) = CUShort((CUShort(response(3 + i * 2)) << 8) Or response(4 + i * 2))
        Next
        TraceMessage(String.Format("READ VALUES 0x{0:X4}, count={1}: {2}", startAddress, count, RegistersToHex(values)))
        Return values
    End Function

    Public Function WaitForCompletion(timeoutMs As Integer,
                                      Optional cancellationRequested As Func(Of Boolean) = Nothing) As UShort
        Dim deadline As DateTime = DateTime.Now.AddMilliseconds(timeoutMs)
        Do
            ThrowIfCancellationRequested(cancellationRequested)
            Dim remaining As Integer = CInt(Math.Max(1, (deadline - DateTime.Now).TotalMilliseconds))
            Dim frame As Byte() = ReadSpecificResponse(CommandEvent, 8, Math.Min(remaining, 1000))
            If frame IsNot Nothing Then
                TraceMessage("RX EVENT: " & BytesToHex(frame))
                Dim registerAddress As UShort = CUShort((CUShort(frame(2)) << 8) Or frame(3))
                Dim status As UShort = CUShort((CUShort(frame(4)) << 8) Or frame(5))
                If registerAddress = StatusEventRegister Then
                    If status = StatusCompleted Then
                        Application.DoEvents()
                        ThrowIfCancellationRequested(cancellationRequested)
                        ReadBasicStatusRegisters()
                        TraceMessage("EVENT STATUS COMPLETED 0x0006")
                        Return status
                    End If
                    If status = StatusError Then
                        TraceMessage("EVENT STATUS ERROR 0x0064")
                        Try
                            StopAndClearLists()
                        Catch
                        End Try
                        Throw New InvalidOperationException("Middle machine reported test error: 0x0064.")
                    End If
                End If
            End If
            Application.DoEvents()
            ThrowIfCancellationRequested(cancellationRequested)
        Loop While DateTime.Now < deadline

        Try
            StopAndClearLists()
        Catch
        End Try
        TraceMessage("RX EVENT TIMEOUT")
        Throw New TimeoutException("Timed out waiting for middle machine test completion.")
    End Function

    Private Sub ThrowIfCancellationRequested(cancellationRequested As Func(Of Boolean))
        If cancellationRequested Is Nothing OrElse Not cancellationRequested() Then Return
        Try
            StopAndClearLists()
        Catch ex As Exception
            TraceMessage("Operator stop cleanup warning: " & ex.Message)
        End Try
        TraceMessage("Test cancelled by operator")
        Throw New OperationCanceledException("测试已由操作员停止。")
    End Sub

    Public Shared Function DescribePlan(plan As MiddleMachineChannelPlan) As String
        If plan Is Nothing Then
            Return "empty plan"
        End If

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0
        Dim mode As String = If(IsSynchronizedPlan(plan), "sync", "independent")
        Dim parts As New List(Of String)()
        parts.Add("mode=" & mode)
        parts.Add("count=" & If(plan.ExecutionOrder Is Nothing, 0, plan.ExecutionOrder.Count).ToString())
        If hasG1 Then
            parts.Add("G1=" & FormatChannelList(plan.G1PhysicalChannels))
            parts.Add("G1Plan=" & If(IsContinuous(New List(Of Integer)(plan.G1PhysicalChannels)), "range", "list"))
        End If
        If hasG2 Then
            Dim g2Local As List(Of Integer) = ToG2LocalChannels(plan.G2PhysicalChannels)
            parts.Add("G2Physical=" & FormatChannelList(plan.G2PhysicalChannels))
            parts.Add("G2Local=" & FormatChannelList(g2Local))
            parts.Add("G2Plan=" & If(IsContinuous(g2Local), "range", "list"))
        End If
        parts.Add("ExecutionOrder=" & FormatChannelList(plan.ExecutionOrder))
        Return String.Join("; ", parts.ToArray())
    End Function

    Public Shared Function BuildWriteFrame(address As UShort, value As UShort) As Byte()
        Dim frame As Byte() = {
            FrameHead,
            CommandWrite,
            CByte((address >> 8) And &HFF),
            CByte(address And &HFF),
            CByte((value >> 8) And &HFF),
            CByte(value And &HFF),
            0,
            0
        }
        AppendCrc(frame)
        Return frame
    End Function

    Public Shared Function BuildReadFrame(startAddress As UShort, count As UShort) As Byte()
        Dim frame As Byte() = {
            FrameHead,
            CommandRead,
            CByte((startAddress >> 8) And &HFF),
            CByte(startAddress And &HFF),
            CByte((count >> 8) And &HFF),
            CByte(count And &HFF),
            0,
            0
        }
        AppendCrc(frame)
        Return frame
    End Function

    Public Shared Function ComputeCrc(data As Byte(), length As Integer) As UShort
        Dim crc As UShort = &HFFFF
        For i As Integer = 0 To length - 1
            crc = CUShort(crc Xor data(i))
            For bit As Integer = 0 To 7
                If (crc And 1) <> 0 Then
                    crc = CUShort((crc >> 1) Xor &HA001)
                Else
                    crc = CUShort(crc >> 1)
                End If
            Next
        Next
        Return crc
    End Function

    Private Sub ConfigureG1Channels(physicalChannels As IList(Of Integer))
        ConfigureGroupChannels(physicalChannels, RegG1PlanMode, RegG1Start, RegG1End, RegG1ListClear, RegG1ListCount, RegG1ListBase)
    End Sub

    Private Sub ConfigureG2Channels(localChannels As IList(Of Integer))
        ConfigureGroupChannels(localChannels, RegG2PlanMode, RegG2Start, RegG2End, RegG2ListClear, RegG2ListCount, RegG2ListBase)
    End Sub

    Private Sub ConfigureGroupChannels(channels As IList(Of Integer),
                                       planModeRegister As UShort,
                                       startRegister As UShort,
                                       endRegister As UShort,
                                       clearRegister As UShort,
                                       countRegister As UShort,
                                       listBase As UShort)
        If channels Is Nothing OrElse channels.Count = 0 Then
            Throw New InvalidOperationException("Group channel list is empty.")
        End If
        If channels.Count > CalibrationService.ChannelCount Then
            Throw New InvalidOperationException("Channel list supports at most 256 channels.")
        End If

        Dim sortedChannels As New List(Of Integer)(channels)
        sortedChannels.Sort()
        If IsContinuous(sortedChannels) Then
            WriteRegister(planModeRegister, &H0)
            WriteRegister(startRegister, CUShort(sortedChannels(0)))
            WriteRegister(endRegister, CUShort(sortedChannels(sortedChannels.Count - 1)))
        Else
            WriteRegister(clearRegister, &H1)
            For i As Integer = 0 To sortedChannels.Count - 1
                ValidateLocalChannel(sortedChannels(i))
                WriteRegister(CUShort(listBase + i), CUShort(sortedChannels(i)))
            Next
            WriteRegister(countRegister, CUShort(sortedChannels.Count))
            WriteRegister(planModeRegister, &H1)
        End If
    End Sub

    Private Sub VerifyPlan(plan As MiddleMachineChannelPlan, speedValue As UShort)
        Dim basic As UShort() = ReadRegisters(RegG1Start, 8)
        Dim advanced As UShort() = ReadRegisters(RegG1PlanMode, 13)

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0

        ExpectRegister(advanced, RegG1PlanMode, RegSyncMode, If(IsSynchronizedPlan(plan), CUShort(0), CUShort(1)))
        ExpectRegister(basic, RegG1Start, RegSpeed, speedValue)
        Dim expectedTwoPass As UShort = CUShort(0)
        ExpectRegister(basic, RegG1Start, RegTwoPass, expectedTwoPass)
        ExpectRegister(advanced, RegG1PlanMode, RegGroupEnable, GetGroupEnableValue(hasG1, hasG2))

        If hasG1 Then
            VerifyGroup(plan.G1PhysicalChannels, False, basic, advanced)
        End If
        ' 同步模式下 G2 跟随 G1，其独立范围寄存器不会单独配置。
        If hasG2 AndAlso Not IsSynchronizedPlan(plan) Then
            VerifyGroup(ToG2LocalChannels(plan.G2PhysicalChannels), True, basic, advanced)
        End If
    End Sub

    Private Sub VerifyGroup(channels As IList(Of Integer), isG2 As Boolean, basic As UShort(), advanced As UShort())
        Dim sortedChannels As New List(Of Integer)(channels)
        sortedChannels.Sort()
        If isG2 Then
            If IsContinuous(sortedChannels) Then
                ExpectRegister(advanced, RegG1PlanMode, RegG2PlanMode, &H0)
                ExpectRegister(advanced, RegG1PlanMode, RegG2Start, CUShort(sortedChannels(0)))
                ExpectRegister(advanced, RegG1PlanMode, RegG2End, CUShort(sortedChannels(sortedChannels.Count - 1)))
            Else
                ExpectRegister(advanced, RegG1PlanMode, RegG2PlanMode, &H1)
                ExpectRegister(advanced, RegG1PlanMode, RegG2ListCount, CUShort(sortedChannels.Count))
            End If
        Else
            If IsContinuous(sortedChannels) Then
                ExpectRegister(advanced, RegG1PlanMode, RegG1PlanMode, &H0)
                ExpectRegister(basic, RegG1Start, RegG1Start, CUShort(sortedChannels(0)))
                ExpectRegister(basic, RegG1Start, RegG1End, CUShort(sortedChannels(sortedChannels.Count - 1)))
            Else
                ExpectRegister(advanced, RegG1PlanMode, RegG1PlanMode, &H1)
                ExpectRegister(advanced, RegG1PlanMode, RegG1ListCount, CUShort(sortedChannels.Count))
            End If
        End If
    End Sub

    Private Shared Sub ExpectRegister(values As UShort(), baseAddress As UShort, address As UShort, expectedValue As UShort)
        Dim offset As Integer = address - baseAddress
        If offset < 0 OrElse offset >= values.Length Then
            Throw New InvalidOperationException("Register verification address is out of range.")
        End If
        If values(offset) <> expectedValue Then
            Throw New InvalidOperationException(String.Format("Middle machine register 0x{0:X4} expected 0x{1:X4}, actual 0x{2:X4}.", address, expectedValue, values(offset)))
        End If
    End Sub

    Private Shared Function IsSynchronizedPlan(plan As MiddleMachineChannelPlan) As Boolean
        If plan Is Nothing Then Return False

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0
        If hasG1 AndAlso Not hasG2 Then Return True
        If Not hasG1 OrElse Not hasG2 Then Return False

        Dim g1 As New List(Of Integer)(plan.G1PhysicalChannels)
        Dim g2 As List(Of Integer) = ToG2LocalChannels(plan.G2PhysicalChannels)
        g1.Sort()
        g2.Sort()
        If Not IsContinuous(g1) OrElse Not IsContinuous(g2) OrElse g1.Count <> g2.Count Then
            Return False
        End If

        For i As Integer = 0 To g1.Count - 1
            If g1(i) <> g2(i) Then Return False
        Next
        Return True
    End Function

    Private Shared Function GetGroupEnableValue(hasG1 As Boolean, hasG2 As Boolean) As UShort
        If hasG1 AndAlso hasG2 Then
            Return &H3
        End If
        If hasG2 Then
            Return &H2
        End If
        Return &H1
    End Function

    Private Shared Function FormatChannelList(channels As IList(Of Integer)) As String
        If channels Is Nothing OrElse channels.Count = 0 Then
            Return "-"
        End If

        Dim sorted As New List(Of Integer)(channels)
        sorted.Sort()
        If IsContinuous(sorted) Then
            If sorted.Count = 1 Then
                Return "CH" & sorted(0).ToString()
            End If
            Return "CH" & sorted(0).ToString() & "-CH" & sorted(sorted.Count - 1).ToString()
        End If

        Dim values As New List(Of String)()
        Dim displayCount As Integer = Math.Min(sorted.Count, 20)
        For i As Integer = 0 To displayCount - 1
            values.Add("CH" & sorted(i).ToString())
        Next
        If sorted.Count > displayCount Then
            values.Add("... total " & sorted.Count.ToString())
        End If
        Return String.Join(",", values.ToArray())
    End Function

    Private Shared Function BytesToHex(bytes As Byte()) As String
        If bytes Is Nothing Then
            Return "(null)"
        End If

        Dim values As New List(Of String)()
        For Each value As Byte In bytes
            values.Add(value.ToString("X2"))
        Next
        Return String.Join(" ", values.ToArray())
    End Function

    Private Shared Function RegistersToHex(values As UShort()) As String
        If values Is Nothing Then
            Return "(null)"
        End If

        Dim parts As New List(Of String)()
        For Each value As UShort In values
            parts.Add("0x" & value.ToString("X4"))
        Next
        Return String.Join(",", parts.ToArray())
    End Function

    Private Shared Function ToG2LocalChannels(physicalChannels As IList(Of Integer)) As List(Of Integer)
        Dim result As New List(Of Integer)()
        For Each physicalChannel As Integer In physicalChannels
            ValidateChannel(physicalChannel)
            If physicalChannel <= 128 Then
                Throw New ArgumentOutOfRangeException("physicalChannels")
            End If
            result.Add(physicalChannel - 128)
        Next
        Return result
    End Function

    Private Shared Function IsContinuous(channelNumbers As IList(Of Integer)) As Boolean
        If channelNumbers.Count <= 1 Then
            Return True
        End If

        For i As Integer = 1 To channelNumbers.Count - 1
            If channelNumbers(i) <> channelNumbers(i - 1) + 1 Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Sub AppendCrc(frame As Byte())
        Dim crc As UShort = ComputeCrc(frame, frame.Length - 2)
        frame(frame.Length - 2) = CByte(crc And &HFF)
        frame(frame.Length - 1) = CByte((crc >> 8) And &HFF)
    End Sub

    Private Function ReadSpecificResponse(command As Byte, expectedLength As Integer, timeoutMs As Integer) As Byte()
        Dim deadline As DateTime = DateTime.Now.AddMilliseconds(timeoutMs)
        Dim buffer As New List(Of Byte)()

        Do
            While serialPort.BytesToRead > 0
                buffer.Add(CByte(serialPort.ReadByte()))
            End While

            Dim frame As Byte() = TryExtractFrame(buffer, command, expectedLength)
            If frame IsNot Nothing Then
                Return frame
            End If

            Thread.Sleep(10)
        Loop While DateTime.Now < deadline

        Return Nothing
    End Function

    Private Shared Function TryExtractFrame(buffer As List(Of Byte), command As Byte, expectedLength As Integer) As Byte()
        Dim i As Integer = 0
        While i <= buffer.Count - expectedLength
            If buffer(i) <> FrameHead Then
                buffer.RemoveAt(i)
                Continue While
            End If

            If buffer(i + 1) <> command Then
                i += 1
                Continue While
            End If

            Dim candidate(expectedLength - 1) As Byte
            buffer.CopyTo(i, candidate, 0, expectedLength)
            If IsValidCrc(candidate) Then
                buffer.RemoveRange(0, i + expectedLength)
                Return candidate
            End If
            i += 1
        End While

        Return Nothing
    End Function

    Private Shared Function IsValidCrc(frame As Byte()) As Boolean
        If frame Is Nothing OrElse frame.Length < 4 Then
            Return False
        End If

        Dim crc As UShort = ComputeCrc(frame, frame.Length - 2)
        Return frame(frame.Length - 2) = CByte(crc And &HFF) AndAlso frame(frame.Length - 1) = CByte((crc >> 8) And &HFF)
    End Function

    Private Shared Function BytesEqual(first As Byte(), second As Byte()) As Boolean
        If first Is Nothing OrElse second Is Nothing OrElse first.Length <> second.Length Then
            Return False
        End If

        For i As Integer = 0 To first.Length - 1
            If first(i) <> second(i) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Sub ValidateChannel(channel As Integer)
        If channel < 1 OrElse channel > CalibrationService.ChannelCount Then
            Throw New ArgumentOutOfRangeException("channel")
        End If
    End Sub

    Private Shared Sub ValidateLocalChannel(channel As Integer)
        If channel < 1 OrElse channel > 128 Then
            Throw New ArgumentOutOfRangeException("channel")
        End If
    End Sub
End Class
