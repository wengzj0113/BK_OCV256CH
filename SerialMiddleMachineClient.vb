Imports System.IO
Imports System.IO.Ports
Imports System.Threading
Imports System.Windows.Forms

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

    Private Const RegStartStop As UShort = &H30
    Private Const RegG1Start As UShort = &H31
    Private Const RegG1End As UShort = &H32
    Private Const RegTwoPass As UShort = &H35
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
    Private Const RegG1ListBase As UShort = &H70
    Private Const RegG2ListBase As UShort = &H100
    Private Const RegWriteResultStart As UShort = &H5B
    Private Const RegBasicStatusStart As UShort = &H40
    Private Const RegBasicStatusCount As UShort = 7

    Private ReadOnly serialPort As SerialPort

    Public Shared Sub LogMessage(message As String)
        Try
            Dim logDir As String = Path.Combine(Application.StartupPath, "logFile")
            If Not Directory.Exists(logDir) Then
                Directory.CreateDirectory(logDir)
            End If

            Dim logPath As String = Path.Combine(logDir, "MiddleMachine_" & DateTime.Now.ToString("yyyyMMdd") & ".log")
            File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & " " & message & Environment.NewLine, System.Text.Encoding.UTF8)
        Catch
        End Try
    End Sub

    Public Sub New(port As SerialPort)
        If port Is Nothing Then
            Throw New ArgumentNullException("port")
        End If

        serialPort = port
        ConfigurePort()
    End Sub

    Public Sub ConfigurePort()
        If String.IsNullOrWhiteSpace(serialPort.PortName) Then
            serialPort.PortName = "COM" & (Initsetting.SCMComPort + 1).ToString()
        End If

        serialPort.BaudRate = If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200)
        serialPort.Parity = Parity.None
        serialPort.DataBits = 8
        serialPort.StopBits = StopBits.One
        serialPort.ReadTimeout = 1000
        serialPort.WriteTimeout = 1000
        LogMessage(String.Format("Serial configured: {0}, {1}, 8N1", serialPort.PortName, serialPort.BaudRate))
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

    Public Sub StopAndClearLists()
        Try
            StopTest()
        Finally
            Try
                ClearChannelLists()
            Catch ex As Exception
                LogMessage("ClearChannelLists failed after STOP: " & ex.Message)
            End Try
        End Try
    End Sub

    Public Sub ClearChannelLists()
        WriteRegister(RegG1ListClear, &H1)
        WriteRegister(RegG2ListClear, &H1)
    End Sub

    Public Sub StartTest()
        WriteRegister(RegStartStop, &H1)
    End Sub

    Public Function RunPlan(plan As MiddleMachineChannelPlan, Optional timeoutMs As Integer = DefaultTimeoutMs) As UShort
        LogMessage("RunPlan: " & DescribePlan(plan))
        EnsureOpen()
        serialPort.DiscardInBuffer()
        ConfigurePlan(plan, True)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Sub ConfigurePlan(plan As MiddleMachineChannelPlan, stableSpeed As Boolean)
        If plan Is Nothing OrElse plan.ExecutionOrder Is Nothing OrElse plan.ExecutionOrder.Count = 0 Then
            Throw New InvalidOperationException("Channel plan is empty.")
        End If

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0
        If Not hasG1 AndAlso Not hasG2 Then
            Throw New InvalidOperationException("Channel plan has no enabled group.")
        End If

        StopTest()
        ClearChannelLists()
        If hasG1 AndAlso Not hasG2 Then
            WriteRegister(RegSyncMode, &H0)
            ConfigureG1Channels(plan.G1PhysicalChannels)
        Else
            WriteRegister(RegSyncMode, &H1)
            WriteRegister(RegGroupEnable, GetGroupEnableValue(hasG1, hasG2))
            If hasG1 Then
                ConfigureG1Channels(plan.G1PhysicalChannels)
            End If
            If hasG2 Then
                ConfigureG2Channels(ToG2LocalChannels(plan.G2PhysicalChannels))
            End If
        End If

        WriteRegister(RegTwoPass, &H0)
        WriteRegister(RegMeasureItems, &HF)
        WriteRegister(RegSpeed, If(stableSpeed, CUShort(1), CUShort(0)))
        VerifyPlan(plan, stableSpeed)
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
        ConfigurePlan(plan, True)
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
        ConfigurePlan(plan, True)
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
        ConfigurePlan(plan, True)
    End Sub

    Public Function RunSingleRange(startChannel As Integer, endChannel As Integer, Optional timeoutMs As Integer = DefaultTimeoutMs) As UShort
        ConfigureSingleRange(startChannel, endChannel)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Function RunChannelList(channelNumbers As IList(Of Integer), Optional timeoutMs As Integer = DefaultTimeoutMs) As UShort
        ConfigureChannelList(channelNumbers)
        StartTest()
        Return WaitForCompletion(timeoutMs)
    End Function

    Public Function RunTwoRanges(firstStartChannel As Integer,
                                 firstEndChannel As Integer,
                                 secondStartChannel As Integer,
                                 secondEndChannel As Integer,
                                 Optional timeoutMs As Integer = DefaultTimeoutMs) As UShort
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

    Private Sub WriteRegister(address As UShort, value As UShort, verifyWriteStatus As Boolean)
        EnsureOpen()
        Dim request As Byte() = BuildWriteFrame(address, value)
        serialPort.DiscardInBuffer()
        LogMessage(String.Format("TX WRITE 0x{0:X4}=0x{1:X4}: {2}", address, value, BytesToHex(request)))
        serialPort.Write(request, 0, request.Length)

        Dim echo As Byte() = ReadSpecificResponse(CommandWrite, 8, DefaultTimeoutMs)
        LogMessage("RX WRITE ECHO: " & BytesToHex(echo))
        If Not BytesEqual(request, echo) Then
            Throw New InvalidOperationException("Middle machine write echo mismatch.")
        End If

        If verifyWriteStatus Then
            VerifyLastWriteStatus(address)
        End If
    End Sub

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

    Public Function ReadRegisters(startAddress As UShort, count As UShort) As UShort()
        If count = 0 OrElse count > 24 Then
            Throw New ArgumentOutOfRangeException("count")
        End If

        EnsureOpen()
        Dim request As Byte() = BuildReadFrame(startAddress, count)
        serialPort.DiscardInBuffer()
        LogMessage(String.Format("TX READ 0x{0:X4}, count={1}: {2}", startAddress, count, BytesToHex(request)))
        serialPort.Write(request, 0, request.Length)

        Dim responseLength As Integer = 3 + count * 2 + 2
        Dim response As Byte() = ReadSpecificResponse(CommandRead, responseLength, DefaultTimeoutMs)
        LogMessage("RX READ: " & BytesToHex(response))
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
        LogMessage(String.Format("READ VALUES 0x{0:X4}, count={1}: {2}", startAddress, count, RegistersToHex(values)))
        Return values
    End Function

    Public Function WaitForCompletion(timeoutMs As Integer) As UShort
        Dim deadline As DateTime = DateTime.Now.AddMilliseconds(timeoutMs)
        Do
            Dim remaining As Integer = CInt(Math.Max(1, (deadline - DateTime.Now).TotalMilliseconds))
            Dim frame As Byte() = ReadSpecificResponse(CommandEvent, 8, Math.Min(remaining, 1000))
            If frame IsNot Nothing Then
                LogMessage("RX EVENT: " & BytesToHex(frame))
                Dim registerAddress As UShort = CUShort((CUShort(frame(2)) << 8) Or frame(3))
                Dim status As UShort = CUShort((CUShort(frame(4)) << 8) Or frame(5))
                If registerAddress = StatusEventRegister Then
                    If status = StatusCompleted Then
                        ReadBasicStatusRegisters()
                        LogMessage("EVENT STATUS COMPLETED 0x0006")
                        Return status
                    End If
                    If status = StatusError Then
                        LogMessage("EVENT STATUS ERROR 0x0064")
                        Try
                            StopAndClearLists()
                        Catch
                        End Try
                        Throw New InvalidOperationException("Middle machine reported test error: 0x0064.")
                    End If
                End If
            End If
            Application.DoEvents()
        Loop While DateTime.Now < deadline

        Try
            StopAndClearLists()
        Catch
        End Try
        Throw New TimeoutException("Timed out waiting for middle machine test completion.")
    End Function

    Public Shared Function DescribePlan(plan As MiddleMachineChannelPlan) As String
        If plan Is Nothing Then
            Return "empty plan"
        End If

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0
        Dim mode As String = If(hasG1 AndAlso Not hasG2, "sync", "independent")
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

    Private Sub VerifyPlan(plan As MiddleMachineChannelPlan, stableSpeed As Boolean)
        Dim basic As UShort() = ReadRegisters(RegG1Start, 8)
        Dim advanced As UShort() = ReadRegisters(RegG1PlanMode, 13)

        Dim hasG1 As Boolean = plan.G1PhysicalChannels IsNot Nothing AndAlso plan.G1PhysicalChannels.Count > 0
        Dim hasG2 As Boolean = plan.G2PhysicalChannels IsNot Nothing AndAlso plan.G2PhysicalChannels.Count > 0

        ExpectRegister(advanced, RegG1PlanMode, RegSyncMode, If(hasG1 AndAlso Not hasG2, CUShort(0), CUShort(1)))
        ExpectRegister(basic, RegG1Start, RegTwoPass, &H0)
        ExpectRegister(advanced, RegG1PlanMode, RegMeasureItems, &HF)
        ExpectRegister(basic, RegG1Start, RegSpeed, If(stableSpeed, CUShort(1), CUShort(0)))

        If hasG1 Then
            VerifyGroup(plan.G1PhysicalChannels, False, basic, advanced)
        End If
        If hasG2 Then
            VerifyGroup(ToG2LocalChannels(plan.G2PhysicalChannels), True, basic, advanced)
            ExpectRegister(advanced, RegG1PlanMode, RegGroupEnable, GetGroupEnableValue(hasG1, hasG2))
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
