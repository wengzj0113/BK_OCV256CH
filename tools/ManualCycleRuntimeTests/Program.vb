Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Threading

Public Module Program
    Public Sub Main()
        TestManualCycleUsesOneSnapshotAndWaitsForR151()
        TestManualCycleUsesSharedNgSortingHandoff()
        TestManualStopCancelsQueuedInitialPass()
        TestManualPreparationFailureDoesNotConfirmTray()
        TestManualR160FailureDoesNotMeasure()
        TestManualStopBeforeR151PreventsMeasurement()
        TestAutomaticCycleStillUsesScannerAndR150()
        Console.WriteLine("PASS: manual/automatic unified PLC cycle runtime")
    End Sub

    Private Sub TestManualStopCancelsQueuedInitialPass()
        Dim plc As New FakePlc()
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {.AutomaticMode = False, .ManualMode = True, .DelayManualStart = True}
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Assert(coordinator.RequestManualCycle("TRAY-1", New Integer() {6}), "manual request must be accepted")
            InvokePoll(coordinator)
            plc.R151 = True
            InvokePoll(coordinator)
            Assert(host.ManualInitialCalls = 0, "a queued UI initial pass must not count as started before its authorization check")

            coordinator.RequestStopCurrentCycle()
            host.ResumeManualInitialPass()
            Assert(host.ManualInitialCalls = 0, "a queued initial pass must be canceled if Stop resets the cycle first")
            Assert(Not coordinator.IsCycleOccupied, "stopping before the queued UI callback runs must release the cycle")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestManualCycleUsesSharedNgSortingHandoff()
        Dim plc As New FakePlc()
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {
            .AutomaticMode = False,
            .ManualMode = True,
            .NeedNgSorting = True,
            .ManualNgChannel = 3
        }
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Assert(coordinator.RequestManualCycle("TRAY-1", New Integer() {3, 7}), "manual sorting cycle must be accepted")
            InvokePoll(coordinator)
            plc.R151 = True
            InvokePoll(coordinator)
            Assert(host.PublishCalls = 1 AndAlso plc.R161WriteCount = 1, "manual final result must publish before the shared R161 handshake")
            Assert(plc.R162WriteCount = 0 AndAlso coordinator.IsCycleOccupied, "manual NG selection must remain pending until R152")

            plc.R151 = False
            plc.R152 = True
            InvokePoll(coordinator)
            Assert(plc.NgSelectionWrites.SequenceEqual(New Integer() {3}), "manual NG handoff must write only the final NG channel grade")
            Assert(plc.R162WriteCount = 1 AndAlso Not coordinator.IsCycleOccupied, "manual NG handoff must finish through R162")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestManualCycleUsesOneSnapshotAndWaitsForR151()
        Dim plc As New FakePlc()
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {.AutomaticMode = False, .ManualMode = True}
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Assert(plc.OpenCount = 1, "manual mode must open the PLC client")
            Assert(scanner.OpenCount = 0, "manual mode must not open the scanner")
            Assert(coordinator.RequestManualCycle("TRAY-1", New Integer() {5, 2}), "first manual request must be accepted")
            Assert(Not coordinator.RequestManualCycle("TRAY-1", New Integer() {2}), "duplicate start must be rejected")
            Assert(host.ManualPrepareCalls = 1 AndAlso String.Join(",", host.PreparedChannels) = "2,5", "preparation must receive the sorted startup snapshot")

            plc.R151 = True
            InvokePoll(coordinator)
            InvokePoll(coordinator)
            Assert(plc.R150ReadCount = 0, "manual polling must not read R150")
            Assert(scanner.ScanCount = 0, "manual flow must not scan")
            Assert(plc.R160WriteCount = 1, "prepared manual tray must be confirmed once")
            Assert(coordinator.State = PlcAutomaticTestState.WaitingFixturePressed, "manual flow must wait for the R151 request after R160 ACK")
            Assert(host.ManualInitialCalls = 0, "a high R151 observed before R160 ACK must not start measurement")

            plc.R151 = False
            InvokePoll(coordinator)
            plc.R151 = True
            InvokePoll(coordinator)
            InvokePoll(coordinator)
            Assert(host.ManualInitialCalls = 1, "one accepted R151 request must start one manual initial pass")
            Assert(String.Join(",", host.InitialChannels) = "2,5", "initial pass must use the frozen manual channel snapshot")
            Assert(host.PublishCalls = 1 AndAlso String.Join(",", host.PublishedChannels) = "2,5", "final publication must retain the manual channel snapshot")
            Assert(plc.R160WriteCount = 1, "waiting and repeated polls must not rewrite R160")
            Assert(Not coordinator.IsCycleOccupied, "a completed cycle must release its coordinator occupancy")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestManualPreparationFailureDoesNotConfirmTray()
        Dim plc As New FakePlc()
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {
            .AutomaticMode = False,
            .ManualMode = True,
            .PreparationFailure = New InvalidOperationException("CH8缺少电芯码")
        }
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Dim failed As Boolean
            Try
                coordinator.RequestManualCycle("TRAY-1", New Integer() {8})
            Catch ex As InvalidOperationException
                failed = ex.Message.Contains("CH8")
            End Try
            Assert(failed, "manual preparation failure must be reported to the caller")
            InvokePoll(coordinator)
            Assert(plc.R160WriteCount = 0, "failed tray preparation must not write R160")
            Assert(host.ManualInitialCalls = 0, "failed tray preparation must not start measurement")
            Assert(Not coordinator.IsCycleOccupied, "pre-R160 preparation failure must release cycle occupancy")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestManualR160FailureDoesNotMeasure()
        Dim plc As New FakePlc() With {.FailR160Count = 1}
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {.AutomaticMode = False, .ManualMode = True}
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Assert(coordinator.RequestManualCycle("TRAY-1", New Integer() {4}), "manual request must be accepted")
            InvokePoll(coordinator)
            Assert(plc.R160WriteCount = 1 AndAlso host.ManualInitialCalls = 0, "R160 failure must not start measurement")
            Assert(coordinator.IsCycleOccupied, "failed R160 confirmation must retain the active tray context")

            InvokePoll(coordinator)
            Assert(plc.R160WriteCount = 2 AndAlso host.ManualInitialCalls = 0, "successful R160 retry must still wait for R151")
            Assert(coordinator.State = PlcAutomaticTestState.WaitingFixturePressed, "R160 retry ACK with R151 low must leave the cycle waiting")
            plc.R151 = True
            InvokePoll(coordinator)
            Assert(host.ManualInitialCalls = 1 AndAlso String.Join(",", host.InitialChannels) = "4", "only the confirmed retry followed by R151 may measure")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestManualStopBeforeR151PreventsMeasurement()
        Dim plc As New FakePlc()
        Dim scanner As New FakeScanner()
        Dim host As New FakeHost() With {.AutomaticMode = False, .ManualMode = True}
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, False)
        Try
            Assert(coordinator.RequestManualCycle("TRAY-1", New Integer() {11}), "manual request must be accepted")
            InvokePoll(coordinator)
            Assert(plc.R160WriteCount = 1 AndAlso host.ManualInitialCalls = 0, "manual cycle must wait for R151 after R160")
            coordinator.RequestStopCurrentCycle()
            plc.R151 = True
            InvokePoll(coordinator)
            Assert(host.ManualInitialCalls = 0 AndAlso plc.R160WriteCount = 1, "stop before R151 must prevent measurement and duplicate R160")
            Assert(Not coordinator.IsCycleOccupied, "stopped cycle must release its occupancy")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Sub TestAutomaticCycleStillUsesScannerAndR150()
        Dim plc As New FakePlc() With {.R150 = True}
        Dim scanner As New FakeScanner() With {.TrayCode = "TRAY-1"}
        Dim host As New FakeHost() With {.AutomaticMode = True, .ManualMode = False}
        Dim coordinator As PlcAutomaticTestCoordinator = StartCoordinator(plc, scanner, host, True)
        Try
            InvokePoll(coordinator)
            Assert(plc.R150ReadCount > 0 AndAlso scanner.OpenCount = 1 AndAlso scanner.ScanCount = 1, "automatic entry must retain R150 and scanner behavior")
            Assert(host.AutomaticPrepareCalls = 1 AndAlso plc.R160WriteCount = 1, "automatic entry must still prepare then confirm the scanned tray")
            plc.R151 = True
            InvokePoll(coordinator)
            Assert(host.AutomaticInitialCalls = 1 AndAlso host.PublishCalls = 1, "automatic entry must use the shared initial-pass and publish path")
        Finally
            coordinator.Stop()
        End Try
    End Sub

    Private Function StartCoordinator(ByVal plc As FakePlc,
                                      ByVal scanner As FakeScanner,
                                      ByVal host As FakeHost,
                                      ByVal automatic As Boolean) As PlcAutomaticTestCoordinator
        Dim coordinator As New PlcAutomaticTestCoordinator(plc, scanner, host)
        If automatic Then
            coordinator.Start()
        Else
            coordinator.StartManual()
        End If
        Dim timerField As FieldInfo = GetType(PlcAutomaticTestCoordinator).GetField("pollTimer", BindingFlags.Instance Or BindingFlags.NonPublic)
        Dim timer As Timer = DirectCast(timerField.GetValue(coordinator), Timer)
        timer.Change(Timeout.Infinite, Timeout.Infinite)
        Return coordinator
    End Function

    Private Sub InvokePoll(ByVal coordinator As PlcAutomaticTestCoordinator)
        Dim poll As MethodInfo = GetType(PlcAutomaticTestCoordinator).GetMethod("Poll", BindingFlags.Instance Or BindingFlags.NonPublic)
        poll.Invoke(coordinator, New Object() {Nothing})
    End Sub

    Private Sub Assert(ByVal condition As Boolean, ByVal message As String)
        If Not condition Then Throw New InvalidOperationException("FAIL: " & message)
    End Sub
End Module

Public NotInheritable Class FakePlc
    Implements IPlcAutomationClient

    Public Property R150 As Boolean
    Public Property R151 As Boolean
    Public Property R152 As Boolean
    Public Property R150ReadCount As Integer
    Public Property R160WriteCount As Integer
    Public Property R161WriteCount As Integer
    Public Property R162WriteCount As Integer
    Public Property OpenCount As Integer
    Public Property FailR160Count As Integer
    Public Property NgSelectionWrites As New List(Of Integer)()
    Public ReadOnly Property IsOpen As Boolean Implements IPlcAutomationClient.IsOpen
        Get
            Return True
        End Get
    End Property

    Public Sub Open() Implements IPlcAutomationClient.Open
        OpenCount += 1
    End Sub
    Public Sub Close() Implements IPlcAutomationClient.Close
    End Sub
    Public Function ReadR150() As Boolean Implements IPlcAutomationClient.ReadR150
        R150ReadCount += 1
        Return R150
    End Function
    Public Function ReadR151() As Boolean Implements IPlcAutomationClient.ReadR151
        Return R151
    End Function
    Public Function ReadR152() As Boolean Implements IPlcAutomationClient.ReadR152
        Return R152
    End Function
    Public Sub WriteR160() Implements IPlcAutomationClient.WriteR160
        R160WriteCount += 1
        If FailR160Count > 0 Then
            FailR160Count -= 1
            Throw New InvalidOperationException("ACK timeout")
        End If
    End Sub
    Public Sub WriteR161() Implements IPlcAutomationClient.WriteR161
        R161WriteCount += 1
    End Sub
    Public Sub WriteR162() Implements IPlcAutomationClient.WriteR162
        R162WriteCount += 1
    End Sub
    Public Sub WriteNgSelection(ByVal channel As Integer) Implements IPlcAutomationClient.WriteNgSelection
        NgSelectionWrites.Add(channel)
    End Sub
End Class

Public NotInheritable Class FakeScanner
    Implements ITrayScannerClient

    Public Property TrayCode As String = "TRAY-1"
    Public Property OpenCount As Integer
    Public Property ScanCount As Integer
    Public ReadOnly Property IsOpen As Boolean Implements ITrayScannerClient.IsOpen
        Get
            Return OpenCount > 0
        End Get
    End Property
    Public Sub Open() Implements ITrayScannerClient.Open
        OpenCount += 1
    End Sub
    Public Sub Close() Implements ITrayScannerClient.Close
    End Sub
    Public Sub TriggerScan() Implements ITrayScannerClient.TriggerScan
        ScanCount += 1
    End Sub
    Public Function ReadTrayCode(ByVal timeoutMs As Integer) As String Implements ITrayScannerClient.ReadTrayCode
        Return TrayCode
    End Function
End Class

Public NotInheritable Class FakeHost
    Implements IAutomaticTestHost
    Implements IManualTestHost

    Public Property AutomaticMode As Boolean
    Public Property ManualMode As Boolean
    Public Property PreparationFailure As Exception
    Public Property NeedNgSorting As Boolean
    Public Property ManualNgChannel As Integer
    Public Property ManualPrepareCalls As Integer
    Public Property AutomaticPrepareCalls As Integer
    Public Property ManualInitialCalls As Integer
    Public Property AutomaticInitialCalls As Integer
    Public Property PublishCalls As Integer
    Public Property PreparedChannels As New List(Of Integer)()
    Public Property InitialChannels As New List(Of Integer)()
    Public Property PublishedChannels As New List(Of Integer)()

    Public Function IsAutomaticModeEnabled() As Boolean Implements IAutomaticTestHost.IsAutomaticModeEnabled
        Return AutomaticMode
    End Function
    Public Function IsDebugModeEnabled() As Boolean Implements IAutomaticTestHost.IsDebugModeEnabled
        Return False
    End Function
    Public Function CanStartAutomaticTest() As Boolean Implements IAutomaticTestHost.CanStartAutomaticTest
        Return AutomaticMode
    End Function
    Public Function ValidateTrayCode(ByVal trayCode As String) As Boolean Implements IAutomaticTestHost.ValidateTrayCode
        Return String.Equals(If(trayCode, String.Empty).Trim(), "TRAY-1", StringComparison.Ordinal)
    End Function
    Public Function GetNeedNgRetest() As Boolean Implements IAutomaticTestHost.GetNeedNgRetest
        Return False
    End Function
    Public Function GetRetestRoundCount() As Integer Implements IAutomaticTestHost.GetRetestRoundCount
        Return 0
    End Function
    Public Function RequiresNgSorting() As Boolean Implements IAutomaticTestHost.RequiresNgSorting
        Return NeedNgSorting
    End Function
    Public Sub SetTrayCode(ByVal trayCode As String) Implements IAutomaticTestHost.SetTrayCode
    End Sub
    Public Sub PrepareAutomaticTray(ByVal trayCode As String) Implements IAutomaticTestHost.PrepareAutomaticTray
        AutomaticPrepareCalls += 1
    End Sub
    Public Sub RequestManualTrayCode(ByVal accepted As Action(Of String), ByVal cancelled As Action) Implements IAutomaticTestHost.RequestManualTrayCode
        accepted("TRAY-1")
    End Sub
    Public Sub StartInitialPass(ByVal completed As Action(Of TestPassResult), ByVal failed As Action(Of Exception)) Implements IAutomaticTestHost.StartInitialPass
        AutomaticInitialCalls += 1
        completed(New TestPassResult With {.Completed = True})
    End Sub
    Public Sub StartRetestPass(ByVal channels As IReadOnlyList(Of Integer), ByVal completed As Action(Of TestPassResult), ByVal failed As Action(Of Exception)) Implements IAutomaticTestHost.StartRetestPass
        Throw New InvalidOperationException("Unexpected automatic retest")
    End Sub
    Public Function PublishFinalCycle(ByVal cycle As AutomaticTestCycle) As Boolean Implements IAutomaticTestHost.PublishFinalCycle
        PublishCalls += 1
        PublishedChannels = cycle.MeasurementChannels.ToList()
        Return True
    End Function
    Public Sub ReportAutomaticStatus(ByVal message As String) Implements IAutomaticTestHost.ReportAutomaticStatus
    End Sub
    Public Sub ReportAutomaticEvent(ByVal message As String) Implements IAutomaticTestHost.ReportAutomaticEvent
    End Sub
    Public Function IsManualModeEnabled() As Boolean Implements IManualTestHost.IsManualModeEnabled
        Return ManualMode
    End Function
    Public Sub EnsureManualTestReady() Implements IManualTestHost.EnsureManualTestReady
        If Not ManualMode Then Throw New InvalidOperationException("Manual mode is not active")
    End Sub
    Public Sub PrepareManualTray(ByVal trayCode As String, ByVal channels As IReadOnlyList(Of Integer)) Implements IManualTestHost.PrepareManualTray
        ManualPrepareCalls += 1
        PreparedChannels = channels.ToList()
        If PreparationFailure IsNot Nothing Then Throw PreparationFailure
    End Sub
    Public Sub StartManualInitialPass(ByVal trayCode As String,
                                      ByVal channels As IReadOnlyList(Of Integer),
                                      ByVal canStart As Func(Of Boolean),
                                      ByVal completed As Action(Of TestPassResult),
                                      ByVal failed As Action(Of Exception)) Implements IManualTestHost.StartManualInitialPass
        Dim startPass As Action =
            Sub()
                If canStart Is Nothing OrElse Not canStart() Then Return
                ManualInitialCalls += 1
                InitialChannels = channels.ToList()
                Dim result As New TestPassResult With {.Completed = True}
                If ManualNgChannel > 0 Then
                    result.Results.Add(New ChannelTestResult With {.ChannelNumber = ManualNgChannel, .IsNg = True})
                End If
                completed(result)
            End Sub
        If DelayManualStart Then
            pendingManualStart = startPass
        Else
            startPass()
        End If
    End Sub

    Private pendingManualStart As Action
    Public Property DelayManualStart As Boolean
    Public Sub ResumeManualInitialPass()
        If pendingManualStart Is Nothing Then Throw New InvalidOperationException("No queued manual initial pass.")
        Dim startPass As Action = pendingManualStart
        pendingManualStart = Nothing
        startPass()
    End Sub
End Class
